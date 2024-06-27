using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace API.Data
{


    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string message)
        {
            var smtpSettings = _configuration.GetSection("Smtp").Get<SmtpSettings>();

            var client = new SmtpClient(smtpSettings.Host, int.Parse(smtpSettings.Port))
            {
                Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                EnableSsl = bool.Parse(smtpSettings.EnableSsl)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.From),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
    }

    public class SmtpSettings
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EnableSsl { get; set; }
        public string From { get; set; }
    }

}