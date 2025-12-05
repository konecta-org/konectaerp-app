using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SharedContracts.Security;

internal sealed class RemoteJwksProvider : IJwtValidationKeysProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<JwtValidationOptions> _optionsMonitor;
    private readonly ILogger<RemoteJwksProvider> _logger;
    private readonly ConcurrentDictionary<string, (DateTimeOffset ExpiresAt, JsonWebKeySet KeySet)> _cache = new();

    public RemoteJwksProvider(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<JwtValidationOptions> optionsMonitor,
        ILogger<RemoteJwksProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public IEnumerable<SecurityKey> GetValidationKeys(string? keyId = null)
    {
        var options = _optionsMonitor.CurrentValue;
        if (string.IsNullOrWhiteSpace(options.JwksUri))
        {
            throw new InvalidOperationException("JwtSettings:JwksUri must be configured.");
        }

        var keySet = GetKeySetAsync(options).GetAwaiter().GetResult();
        if (keySet is null)
        {
            return Array.Empty<SecurityKey>();
        }

        var keys = keySet.Keys
            .Where(k => string.IsNullOrWhiteSpace(keyId) || string.Equals(k.Kid, keyId, StringComparison.Ordinal))
            .Cast<SecurityKey>()
            .ToArray();

        return keys.Length > 0 ? keys : keySet.Keys.Cast<SecurityKey>();
    }

    private async Task<JsonWebKeySet?> GetKeySetAsync(JwtValidationOptions options)
    {
        var cacheKey = options.JwksUri.Trim();
        if (_cache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return cached.KeySet;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("jwks");
            client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true
            };

            using var response = await client.GetAsync(cacheKey);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var keySet = new JsonWebKeySet(content);

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(Math.Max(1, options.CacheDurationMinutes));
            _cache[cacheKey] = (expiresAt, keySet);

            return keySet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve JWKS from {Uri}", options.JwksUri);
            return null;
        }
    }
}
