using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AuthenticationService.Options;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public List<JwtSigningKeyOptions> Keys { get; set; } = new();

    /// <summary>
    /// Returns the key marked as current. Falls back to the first key if no flag is set.
    /// </summary>
    public JwtSigningKeyOptions GetCurrentKey()
    {
        if (Keys.Count == 0)
        {
            throw new InvalidOperationException("JwtSettings:Keys must contain at least one entry.");
        }

        return Keys.FirstOrDefault(k => k.IsCurrent) ?? Keys[0];
    }

    /// <summary>
    /// Returns an immutable snapshot of all configured keys.
    /// </summary>
    public ImmutableArray<JwtSigningKeyOptions> GetAllKeys() => [.. Keys];
}

public sealed class JwtSigningKeyOptions
{
    public string Id { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public string? PrivateKeyPem { get; set; }
    public string? PublicKeyPem { get; set; }
    public DateTimeOffset? NotBefore { get; set; }
    public DateTimeOffset? NotAfter { get; set; }
}
