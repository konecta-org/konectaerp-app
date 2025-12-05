using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public interface IUserRepository
    {
        Task<PagedResultDto<User>> GetPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<UserSummaryData> GetSummaryAsync(CancellationToken cancellationToken = default);

        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        void Update(User user);
    }
}
