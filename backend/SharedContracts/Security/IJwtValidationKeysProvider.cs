using Microsoft.IdentityModel.Tokens;

namespace SharedContracts.Security;

public interface IJwtValidationKeysProvider
{
    IEnumerable<SecurityKey> GetValidationKeys(string? keyId = null);
}
