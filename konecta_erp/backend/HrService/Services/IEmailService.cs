namespace HrService.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string employeeName, string workEmail, string temporaryPassword);
    }
}
