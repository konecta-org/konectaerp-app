namespace SharedContracts.Security;

public sealed class JwtValidationOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string[]? AdditionalAudiences { get; set; }
    public string[]? AdditionalIssuers { get; set; }
    public string JwksUri { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = false;
    public int CacheDurationMinutes { get; set; } = 30;
}
