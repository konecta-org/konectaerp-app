using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthenticationService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedContracts.Security;

namespace AuthenticationService.Services;

public sealed class JwksProvider : IJwksProvider, IDisposable
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IOptionsMonitor<JwtOptions> _optionsMonitor;
    private readonly ILogger<JwksProvider> _logger;
    private readonly IDisposable? _onChangeToken;
    private readonly object _sync = new();

    private SigningCredentials? _signingCredentials;
    private IReadOnlyDictionary<string, SecurityKey> _validationKeys = new Dictionary<string, SecurityKey>();
    private IReadOnlyCollection<JsonWebKey> _jsonWebKeys = Array.Empty<JsonWebKey>();
    private JsonWebKeySet _jsonWebKeySet = new("{}");
    private string _currentKeyId = string.Empty;
    private bool _disposed;

    public JwksProvider(IOptionsMonitor<JwtOptions> optionsMonitor, ILogger<JwksProvider> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _onChangeToken = _optionsMonitor.OnChange(HandleOptionsChanged);
        Reload(optionsMonitor.CurrentValue);
    }

    public SigningCredentials GetCurrentSigningCredentials()
    {
        ThrowIfDisposed();

        if (_signingCredentials is null)
        {
            lock (_sync)
            {
                if (_signingCredentials is null)
                {
                    Reload(_optionsMonitor.CurrentValue);
                }
            }
        }

        return _signingCredentials ?? throw new InvalidOperationException("Signing credentials are not configured.");
    }

    public JsonWebKeySet GetJwksDocument()
    {
        ThrowIfDisposed();
        return _jsonWebKeySet;
    }

    public JwtSigningKeySnapshot GetSnapshot()
    {
        ThrowIfDisposed();
        return new JwtSigningKeySnapshot(_currentKeyId, _jsonWebKeys);
    }

    public IEnumerable<SecurityKey> GetValidationKeys(string? keyId = null)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return _validationKeys.Values;
        }

        return _validationKeys.TryGetValue(keyId, out var key)
            ? new[] { key }
            : _validationKeys.Values;
    }

    private void HandleOptionsChanged(JwtOptions options, string? _)
    {
        Reload(options);
    }

    private void Reload(JwtOptions options)
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            var keys = options.GetAllKeys();
            if (keys.Length == 0)
            {
                throw new InvalidOperationException("JwtSettings.Keys must contain at least one entry.");
            }

            var validationKeyMap = new Dictionary<string, SecurityKey>(StringComparer.Ordinal);
            var jsonKeys = new List<JsonWebKey>(keys.Length);
            RsaSecurityKey? signingKey = null;
            JwtSigningKeyOptions? currentKey = null;

            foreach (var keyOptions in keys)
            {
                if (string.IsNullOrWhiteSpace(keyOptions.Id))
                {
                    throw new InvalidOperationException("JwtSettings.Keys entries must have a non-empty Id.");
                }

                // Create RSA for validation (public key only if private not available)
                var validationSecurityKey = CreateRsaSecurityKey(keyOptions, includePrivate: false);
                validationSecurityKey.KeyId = keyOptions.Id;
                validationKeyMap[keyOptions.Id] = validationSecurityKey;

                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(validationSecurityKey);
                jwk.Kid = keyOptions.Id;
                jwk.Use = JsonWebKeyUseNames.Sig;
                jwk.Alg = SecurityAlgorithms.RsaSha256;
                jsonKeys.Add(jwk);

                if (keyOptions.IsCurrent)
                {
                    currentKey = keyOptions;
                }
            }

            currentKey ??= keys[0];

            var currentSigningRsaKey = CreateRsaSecurityKey(currentKey, includePrivate: true);
            if (currentSigningRsaKey.Rsa is null && currentSigningRsaKey.PrivateKeyStatus != PrivateKeyStatus.Exists)
            {
                throw new InvalidOperationException($"Current signing key '{currentKey.Id}' does not contain a private key.");
            }

            currentSigningRsaKey.KeyId = currentKey.Id;
            signingKey = currentSigningRsaKey;

            _signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
            _validationKeys = validationKeyMap;
            _jsonWebKeys = jsonKeys;
            _jsonWebKeySet = new JsonWebKeySet(JsonSerializer.Serialize(
                new JsonWebKeySetPayload(jsonKeys), JsonSerializerOptions));
            _currentKeyId = currentKey.Id;

            _logger.LogInformation("JWT signing keys reloaded. Current key id: {KeyId}.", _currentKeyId);
        }
    }

    private static RsaSecurityKey CreateRsaSecurityKey(JwtSigningKeyOptions keyOptions, bool includePrivate)
    {
        RSA rsa = RSA.Create();
        var source = includePrivate ? keyOptions.PrivateKeyPem : keyOptions.PublicKeyPem;

        if (string.IsNullOrWhiteSpace(source) && !includePrivate)
        {
            source = keyOptions.PrivateKeyPem;
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            rsa.ImportFromPem(source);
        }
        else if (!string.IsNullOrWhiteSpace(keyOptions.PublicKeyPem))
        {
            rsa.ImportFromPem(keyOptions.PublicKeyPem);
        }
        else
        {
            throw new InvalidOperationException($"Key '{keyOptions.Id}' is missing PEM material.");
        }

        return new RsaSecurityKey(rsa) { KeyId = keyOptions.Id };
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(JwksProvider));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _onChangeToken?.Dispose();
        if (_signingCredentials?.Key is RsaSecurityKey signingKey && signingKey.Rsa is not null)
        {
            signingKey.Rsa.Dispose();
        }

        foreach (var key in _validationKeys.Values.OfType<RsaSecurityKey>())
        {
            key.Rsa?.Dispose();
        }
    }

    private readonly record struct JsonWebKeySetPayload(IReadOnlyCollection<JsonWebKey> Keys);
}

