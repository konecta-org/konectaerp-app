using AuthenticationService.Dtos;

namespace AuthenticationService.Services;

public interface IUserManagementClient
{
    Task<AuthorizationProfileDto?> GetAuthorizationProfileAsync(string userId, CancellationToken cancellationToken = default);
}
