using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync(bool includePermissions = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Role> query = _context.Roles;

            if (includePermissions)
            {
                query = query
                    .Include(role => role.Permissions)!
                        .ThenInclude(rp => rp.Permission);
            }

            query = query.Include(role => role.UserRoles);

            return await query
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Role?> GetByIdAsync(int id, bool includePermissions = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Role> query = _context.Roles.Where(role => role.Id == id);

            if (includePermissions)
            {
                query = query
                    .Include(role => role.Permissions)!
                        .ThenInclude(rp => rp.Permission);
            }

            query = query.Include(role => role.UserRoles)!;

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Role?> GetByNameAsync(string name, bool includePermissions = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Role> query = _context.Roles.Where(role => role.Name == name);

            if (includePermissions)
            {
                query = query
                    .Include(role => role.Permissions)!
                        .ThenInclude(rp => rp.Permission);
            }

            query = query.Include(role => role.UserRoles)!;

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Role>> GetByIdsAsync(IEnumerable<int> ids, bool includePermissions = false, CancellationToken cancellationToken = default)
        {
            var idList = ids.Distinct().ToList();

            if (idList.Count == 0)
            {
                return Array.Empty<Role>();
            }

            IQueryable<Role> query = _context.Roles.Where(role => idList.Contains(role.Id));

            if (includePermissions)
            {
                query = query
                    .Include(role => role.Permissions)!
                        .ThenInclude(rp => rp.Permission);
            }

            query = query.Include(role => role.UserRoles)!;

            return await query.ToListAsync(cancellationToken);
        }

        public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Role> query = _context.Roles;

            if (excludeId.HasValue)
            {
                query = query.Where(role => role.Id != excludeId.Value);
            }

            return query.AnyAsync(role => role.Name == name, cancellationToken);
        }

        public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            await _context.Roles.AddAsync(role, cancellationToken);
        }

        public void Update(Role role)
        {
            _context.Roles.Update(role);
        }

        public void Remove(Role role)
        {
            _context.Roles.Remove(role);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
