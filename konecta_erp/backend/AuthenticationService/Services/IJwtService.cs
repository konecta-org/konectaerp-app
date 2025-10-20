using System.Security.Claims;
using AuthenticationService.Models;

namespace AuthenticationService.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user);
        ClaimsPrincipal? ValidateToken(string token);

    }
}
