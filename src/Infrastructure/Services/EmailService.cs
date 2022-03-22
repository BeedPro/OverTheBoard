﻿using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OverTheBoard.ObjectModel;

namespace OverTheBoard.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettingOptions _options;

        public EmailService(IOptions<EmailSettingOptions> options)
        {
            _options = options.Value;
        }
        public async Task<bool> SendRegistrationEmailAsync(string email, string displayName,string link)
        {
            using (var smtpClient = new SmtpClient(_options.SmtpServer))
            {
                smtpClient.Port = 25;
                smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);
                smtpClient.EnableSsl = false;

                var body = $"<h3>OverTheBoard</h3>\r\n<p>Hello {displayName}. Welcome to OverTheBoard where you play Chess in a fun and gamified Tournament based system. To verify your account please click the <a href=\"{link}\">link</a>.";

                var mailMessage = new MailMessage()
                {
                    From = new MailAddress(_options.Sender),
                    Subject = "OverTheBoard Email Verification",
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);

            }
            return true;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string displayName,string token, string callback)
        {
            using (var smtpClient = new SmtpClient(_options.SmtpServer))
            {
                smtpClient.Port = 25;
                smtpClient.Credentials = new NetworkCredential(_options.Username, _options.Password);
                smtpClient.EnableSsl = false;
                var link = callback;
                var body = $"<h3>OverTheBoard</h3>\r\n<p>Hello {displayName}. Welcome to OverTheBoard where you play Chess in a fun and gamified Tournament based system. To reset your account password please click the <a href=\"{link}\">link</a>.";

                var mailMessage = new MailMessage()
                {
                    From = new MailAddress(_options.Sender),
                    Subject = "OverTheBoard Password Reset",
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);

            }
            return true;
        }
    }
}
