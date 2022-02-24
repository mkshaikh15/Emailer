# Code Project - Emailer.DLL class files

This is a web API which sends emails through form-data retrieved from a controller.

This repo contains the necessary files for the Emailer.DLL to compile.

The Emailer also writes logs, the logger settings can be configured in AppSettings.json (IE, retry N amount of times after every X amount of milliseconds)

It can easily be modified to work with a GUI/Windows Form Application by utilizing an action/event listener on a button click "send"
Simply get rid of the HttpPost and [FromForm] before the MailRequest object is populated when the POST method is sent.
Instead, call the [HttpPost] function on a button click and populate the MailRequest object from the form fields.

Libraries/Nuget Packages utilized in this project: .

`Logger.dll` simple DLL created to create Logs to use, see Logging project README.md on how to use

`MailKit` standard Emailing library,  System.Net.Mail functionality could be utilized in place of this, but after doing research I decided this was the best practice to go with

## Usage

    public async Task SendEmailAsync(MailRequest mailRequest);
    Call it by: await SendEmailAsync(MailRequest mailRequest)
    It is populated through form-data from a controller. See Project README.md. Make sure to reference the DLL from this library to test it out.
    
## Compile and reference the DLL

    Pull this project or import it into Visual Studio (I used 2019)
    Build the project, make sure you have the Libraries that are utilized in this project
    Libraries: MailKit, Microsoft.Extensions.Options, and Logger.DLL (See Logger project README.md)