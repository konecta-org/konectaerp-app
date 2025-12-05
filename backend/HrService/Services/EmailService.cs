using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace HrService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string employeeName, string workEmail, string temporaryPassword)
        {
            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                var fromEmail = _configuration["SendGrid:FromEmail"];
                var fromName = _configuration["SendGrid:FromName"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("SendGrid API Key is not configured. Email will not be sent.");
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail, employeeName);
                var subject = "Welcome to Konecta ERP!";
                
                var plainTextContent = $@"
Hello {employeeName},

Welcome to Konecta ERP! Your employee account has been created.

Your login credentials are:
Email: {workEmail}
Temporary Password: {temporaryPassword}

Please login to the system and change your password immediately.

Login URL: http://localhost:8080

Best regards,
Konecta ERP Team
";

                var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f4f4f4; padding: 20px; }}
        .credentials {{ background-color: white; padding: 15px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
        .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; display: inline-block; margin-top: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Konecta ERP!</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{employeeName}</strong>,</p>
            <p>We're excited to welcome you to Konecta ERP! Your employee account has been successfully created.</p>
            
            <div class='credentials'>
                <h3>Your Login Credentials:</h3>
                <p><strong>Email:</strong> {workEmail}</p>
                <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
            </div>
            
            <p><strong>⚠️ Important:</strong> Please login to the system and change your password immediately for security purposes.</p>
            
            <a href='http://localhost:8080' class='button'>Login Now</a>
        </div>
        <div class='footer'>
            <p>This is an automated message from Konecta ERP System.</p>
            <p>If you have any questions, please contact your system administrator.</p>
        </div>
    </div>
</body>
</html>
";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
                }
                else
                {
                    _logger.LogError("Failed to send email to {Email}. Status: {Status}", toEmail, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
            }
        }
    }
}
