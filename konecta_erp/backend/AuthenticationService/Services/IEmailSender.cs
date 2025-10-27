namespace AuthenticationService.Services
{
    public interface IEmailSender
    {
        Task SendEmployeeCredentialsAsync(string toPersonalEmail, string fullName, string workEmail, string password);
    }
}
