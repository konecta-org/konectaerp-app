using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync(bool includePermissions = false, CancellationToken cancellationToken = default);
        Task<Role?> GetByIdAsync(int id, bool includePermissions = false, CancellationToken cancellationToken = default);
        Task<Role?> GetByNameAsync(string name, bool includePermissions = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<Role>> GetByIdsAsync(IEnumerable<int> ids, bool includePermissions = false, CancellationToken cancellationToken = default);
        Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
        Task AddAsync(Role role, CancellationToken cancellationToken = default);
        void Update(Role role);
        void Remove(Role role);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
