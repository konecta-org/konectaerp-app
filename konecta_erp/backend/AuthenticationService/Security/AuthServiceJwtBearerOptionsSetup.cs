using AuthenticationService.Options;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Security;

public sealed class AuthServiceJwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IJwksProvider _jwksProvider;
    private readonly IOptions<JwtOptions> _jwtOptions;

    public AuthServiceJwtBearerOptionsSetup(
        IJwksProvider jwksProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _jwksProvider = jwksProvider;
        _jwtOptions = jwtOptions;
    }

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    public void Configure(JwtBearerOptions options)
    {
        var settings = _jwtOptions.Value;

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                _jwksProvider.GetValidationKeys(kid).ToArray()
        };
    }
}
