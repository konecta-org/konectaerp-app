using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmployeeCredentialsAsync(string toPersonalEmail, string fullName, string workEmail, string password)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
                Subject = "Your Konecta ERP account credentials",
                Body = BuildBody(fullName, workEmail, password),
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(toPersonalEmail, fullName));

            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.UseSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.UserName) && !string.IsNullOrWhiteSpace(_options.Password))
            {
                client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
            }

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send onboarding email to {Recipient}", toPersonalEmail);
                throw;
            }
        }

        private static string BuildBody(string fullName, string workEmail, string password)
        {
            return $@"Hi {fullName},

Welcome to Konecta ERP!

We have created your work account. Here are your credentials:
 - Work email: {workEmail}
 - Temporary password: {password}

Please sign in as soon as possible and change your password.

If you have issues signing in, reach out to the IT support team.

Regards,
Konecta HR";
        }
    }
}
