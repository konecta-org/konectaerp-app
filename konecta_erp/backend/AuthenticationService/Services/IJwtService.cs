using System.Security.Claims;
using AuthenticationService.Models;

namespace AuthenticationService.Services
{
    public interface IJwtService
    {
        TokenResult GenerateToken(ApplicationUser user, IEnumerable<string> roles);
        ClaimsPrincipal? ValidateToken(string token);

    }
}
