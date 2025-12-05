namespace SharedContracts.Configuration;

public class ServiceAuthOptions
{
    public const string SectionName = "ServiceAuth";

    /// <summary>
    /// Shared secret for trusted service-to-service communication (e.g., AuthenticationService -> UserManagementService).
    /// </summary>
    public string SharedSecret { get; set; } = string.Empty;
    public string? UserManagementBaseUrl { get; set; }
}
