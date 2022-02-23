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

namespace Emailer.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> options)
        {
            _mailSettings = options.Value;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_mailSettings.Email);
            message.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            message.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
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
            int counter = 0;
            do
            {
                try
                {
                    smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_mailSettings.Email, _mailSettings.Password);
                    await smtp.SendAsync(message);
                    Console.WriteLine("Email sent successfully after " + counter + " reattempt(s)");
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Email failed to send. Trying again after " + ++counter + " attempt(s)");
                    await Task.Delay(_mailSettings.RetryTimer);
                }

            } while (counter < _mailSettings.RetryCount);
            message.Dispose();
            smtp.Disconnect(true);
        }
    }
}
