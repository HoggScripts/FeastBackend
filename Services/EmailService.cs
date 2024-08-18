using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Feast.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Feast", _configuration["EmailSettings:SmtpUser"]));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = body };

            using (var client = new SmtpClient())
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];

                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendRegistrationEmailAsync(string to, string username, string confirmationLink)
        {
            var subject = "Welcome to Feast!";
            var body = BuildRegistrationEmailBody(username, confirmationLink);
            await SendEmailAsync(to, subject, body);
        }

        private string BuildRegistrationEmailBody(string username, string confirmationLink)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f6f6f6;
                        margin: 0;
                        padding: 20px;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: auto;
                        background-color: #ffffff;
                        padding: 20px;
                        border-radius: 8px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        background-color: #007BFF;
                        color: white;
                        padding: 10px 20px;
                        text-align: center;
                        border-radius: 8px 8px 0 0;
                    }}
                    .content {{
                        padding: 20px;
                        line-height: 1.6;
                        color: #333333;
                    }}
                    .content h1 {{
                        color: #007BFF;
                    }}
                    .footer {{
                        padding: 10px 20px;
                        text-align: center;
                        font-size: 12px;
                        color: #777777;
                    }}
                    .footer a {{
                        color: #007BFF;
                        text-decoration: none;
                    }}
                    .btn {{
                        display: inline-block;
                        padding: 10px 20px;
                        margin-top: 20px;
                        background-color: #007BFF;
                        color: white;
                        text-decoration: none;
                        border-radius: 5px;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='header'>
                        <h1>Welcome to Feast, {username}!</h1>
                    </div>
                    <div class='content'>
                        <h2>Thank you for registering with us!</h2>
                        <p>
                            We're excited to have you on board. Feast is your ultimate solution for planning and organizing your meals with ease.
                        </p>
                        <p>
                            To confirm your email address, please click the button below:
                        </p>
                        <a href='{confirmationLink}' class='btn'>Confirm Email</a>
                        <p>If you did not register for this account, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Feast. All rights reserved.</p>
                        <p><a href='#'>Unsubscribe</a></p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
