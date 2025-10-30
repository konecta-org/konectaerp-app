using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public interface IUserService
    {
        Task<PagedResultDto<User>> GetUsersAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User> CreateAsync(UserCreateDto dto, CancellationToken cancellationToken = default);
        Task<User?> UpdateAsync(string id, UserUpdateDto dto, CancellationToken cancellationToken = default);
        Task<bool> ChangeRoleAsync(string id, RoleChangeDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateStatusAsync(string id, UserStatusUpdateDto dto, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> RestoreAsync(string id, CancellationToken cancellationToken = default);
        Task<UserSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
        Task<User> CreateOrUpdateFromExternalAsync(string externalUserId, string email, string fullName, string role, CancellationToken cancellationToken = default);
    }
}
