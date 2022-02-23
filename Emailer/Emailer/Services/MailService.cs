using MailKit.Net.Smtp;
using MimeKit;
using Emailer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Security;
using Emailer.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Text;
using Logging;

namespace Emailer.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private int counter { get; set; }
        public MailService(IOptions<MailSettings> options)
        {
            _mailSettings = options.Value;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(_mailSettings.LogPath);
                if (!dir.Exists)
                    dir.Create();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error verifying the directory path. Check configuration for LogPath in appsettings.json." + ex.Message);
            }
        }


        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            counter = 0;
            using var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_mailSettings.Email);
            message.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            message.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (IFormFile file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            builder.HtmlBody = mailRequest.Body;
            message.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            do
            {
                try
                {
                    smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_mailSettings.Email, _mailSettings.Password);
                    await smtp.SendAsync(message);
                    Console.WriteLine("Email sent successfully after " + counter + " reattempt(s)");
                    StringBuilder sb = new StringBuilder();
                    String logText = sb.Append("Email successfully sent at ").AppendLine(DateTime.Now.ToString()).Append("\tSender: ").AppendLine(_mailSettings.Email).Append("\tRecipient: ").AppendLine(mailRequest.ToEmail).Append("\tSubject: ").AppendLine(mailRequest.Subject).Append("\t").AppendLine(mailRequest.Body).ToString();
                    Logger.Log(logText, _mailSettings.LogPath, _mailSettings.LogFileName);
                    break;
                }
                catch (Exception)
                {
                    //Email sender, recipient, subject, and body
                    StringBuilder sb = new StringBuilder();
                    String logText = sb.Append("Email failed to send at ").AppendLine(DateTime.Now.ToString()).Append("\tSender: ").AppendLine(_mailSettings.Email).Append("\tRecipient: ").AppendLine(mailRequest.ToEmail).Append("\tSubject: ").AppendLine(mailRequest.Subject).Append("\t").AppendLine(mailRequest.Body).ToString();
                    //Console.WriteLine("Email failed to send. Trying again after " + ++counter + " attempt(s)");
                    Logger.Log(logText, _mailSettings.LogPath, _mailSettings.LogFileName);
                    await Task.Delay(_mailSettings.RetryTimer);

                    if (counter == _mailSettings.RetryCount)
                    {
                        message.Dispose();
                        smtp.Disconnect(true);
                        Logger.Log("Stopped sending email after: " + counter + " attempts\n", _mailSettings.LogPath, _mailSettings.LogFileName);
                    }
                }

            } while (counter < _mailSettings.RetryCount);
        }
    }
}
