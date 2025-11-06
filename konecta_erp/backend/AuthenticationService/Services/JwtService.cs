using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationService.Models;
using AuthenticationService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Services;

public sealed class JwtService : IJwtService
{
    private readonly IJwksProvider _jwksProvider;
    private readonly IOptionsMonitor<JwtOptions> _optionsMonitor;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        IJwksProvider jwksProvider,
        IOptionsMonitor<JwtOptions> optionsMonitor,
        ILogger<JwtService> logger)
    {
        _jwksProvider = jwksProvider;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public TokenResult GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);

        var options = _optionsMonitor.CurrentValue;
        var signingCredentials = _jwksProvider.GetCurrentSigningCredentials();
        var expiresAt = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, ToUnixTimeSeconds(DateTime.UtcNow), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, user.Id),
            new("uid", user.Id),
            new("full_name", user.FullName ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            claims.Add(new Claim("preferred_username", user.UserName));
        }

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("employee_id", user.EmployeeId.Value.ToString()));
        }

        var distinctRoles = roles?
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        foreach (var role in distinctRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("roles", role));
        }

        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = signingCredentials
        };

        var securityToken = handler.CreateJwtSecurityToken(descriptor);
        securityToken.Header["kid"] = signingCredentials.Key.KeyId;

        var token = handler.WriteToken(securityToken);
        return new TokenResult(token, expiresAt, signingCredentials.Key.KeyId ?? string.Empty);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler();
        var options = _optionsMonitor.CurrentValue;

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKeyResolver = (_, _, kid, _) =>
                _jwksProvider.GetValidationKeys(kid).ToArray()
        };

        try
        {
            return handler.ValidateToken(token, validationParameters, out _);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "JWT validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while validating JWT.");
            return null;
        }
    }

    private static string ToUnixTimeSeconds(DateTime value)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        return unixTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
