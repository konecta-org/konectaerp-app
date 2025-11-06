using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SharedContracts.Security;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtValidationOptions>(configuration.GetSection(JwtValidationOptions.SectionName));
        services.AddHttpClient("jwks");
        services.AddSingleton<IJwtValidationKeysProvider, RemoteJwksProvider>();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, RemoteJwtBearerOptionsSetup>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        return services;
    }

    private sealed class RemoteJwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IJwtValidationKeysProvider _keyProvider;
        private readonly IOptions<JwtValidationOptions> _options;

        public RemoteJwtBearerOptionsSetup(
            IJwtValidationKeysProvider keyProvider,
            IOptions<JwtValidationOptions> options)
        {
            _keyProvider = keyProvider;
            _options = options;
        }

        public void Configure(string? name, JwtBearerOptions options) => Configure(options);

        public void Configure(JwtBearerOptions options)
        {
            var settings = _options.Value;

            options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidIssuers = settings.AdditionalIssuers?.Any() == true
                    ? new[] { settings.Issuer }.Concat(settings.AdditionalIssuers!).Distinct().ToArray()
                    : null,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidAudiences = settings.AdditionalAudiences?.Any() == true
                    ? new[] { settings.Audience }.Concat(settings.AdditionalAudiences!).Distinct().ToArray()
                    : null,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKeyResolver = (_, _, kid, _) =>
                    _keyProvider.GetValidationKeys(kid).ToArray()
            };
        }
    }
}
