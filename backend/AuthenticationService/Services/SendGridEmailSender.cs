using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AuthenticationService.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(IOptions<SendGridOptions> options, ILogger<SendGridEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmployeeCredentialsAsync(string toPersonalEmail, string fullName, string workEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning("SendGrid API key is not configured; skipping email send for {Recipient}", toPersonalEmail);
                return;
            }

            if (string.IsNullOrWhiteSpace(toPersonalEmail))
            {
                _logger.LogWarning("Personal email is missing for employee {FullName}; skipping onboarding email.", fullName);
                return;
            }

            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(toPersonalEmail, fullName);
            var subject = "Your Konecta ERP account credentials";
            var plainTextContent = BuildBody(fullName, workEmail, password);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent: null);

            try
            {
                var response = await client.SendEmailAsync(msg);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Sent onboarding email to {Recipient}", toPersonalEmail);
                }
                else
                {
                    var body = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send onboarding email to {Recipient}. StatusCode: {StatusCode}, Body: {Body}", toPersonalEmail, response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendGrid failed to send onboarding email to {Recipient}", toPersonalEmail);
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
