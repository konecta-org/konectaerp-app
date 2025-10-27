namespace AuthenticationService.Services
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string FromAddress { get; set; } = "no-reply@konecta.local";
        public string FromDisplayName { get; set; } = "Konecta HR";
        public string SmtpHost { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 25;
        public bool UseSsl { get; set; } = false;
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
