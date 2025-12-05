using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Permissions
                .AsNoTracking()
                .OrderBy(permission => permission.Category)
                .ThenBy(permission => permission.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Permissions.FirstOrDefaultAsync(permission => permission.Id == id, cancellationToken);
        }

        public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Permission> query = _context.Permissions;

            if (excludeId.HasValue)
            {
                query = query.Where(permission => permission.Id != excludeId.Value);
            }

            return query.AnyAsync(permission => permission.Name == name, cancellationToken);
        }

        public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            await _context.Permissions.AddAsync(permission, cancellationToken);
        }

        public void Update(Permission permission)
        {
            _context.Permissions.Update(permission);
        }

        public void Remove(Permission permission)
        {
            _context.Permissions.Remove(permission);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
