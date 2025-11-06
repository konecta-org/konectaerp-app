using Microsoft.IdentityModel.Tokens;
using SharedContracts.Security;

namespace AuthenticationService.Services;

public interface IJwksProvider : IJwtValidationKeysProvider
{
    SigningCredentials GetCurrentSigningCredentials();
    JsonWebKeySet GetJwksDocument();
    JwtSigningKeySnapshot GetSnapshot();
}

public sealed record JwtSigningKeySnapshot(string CurrentKeyId, IReadOnlyCollection<JsonWebKey> Keys);
