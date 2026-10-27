using System.Net;
using System.Net.Mail;
using ChurchPortal.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ChurchPortal.Services.Implementations
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;

        public SmtpEmailSender(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.Host) && !string.IsNullOrWhiteSpace(_options.From);

        public async Task SendAsync(string to, string subject, string body)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    "Email delivery is not configured on the server. Set an SMTP host and From address and try again.");
            }

            using var client = new SmtpClient(_options.Host!, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            using var message = new MailMessage(_options.From!, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
