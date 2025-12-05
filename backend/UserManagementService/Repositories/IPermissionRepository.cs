using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
        Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
        void Update(Permission permission);
        void Remove(Permission permission);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
