using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<User>> GetPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(user => user.UserRoles)!
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!parameters.IncludeDeleted)
            {
                query = query.Where(user => !user.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Role))
            {
                var roleName = parameters.Role.Trim();
                query = query.Where(user => user.UserRoles!.Any(ur => ur.Role.Name == roleName));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Department))
            {
                query = query.Where(user => user.Department != null && user.Department == parameters.Department);
            }

            if (parameters.OnlyLocked)
            {
                query = query.Where(user => user.IsLocked);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var rawSearch = parameters.Search.Trim();
                var searchTerm = rawSearch.ToUpperInvariant();
                query = query.Where(user =>
                    user.NormalizedEmail.Contains(searchTerm) ||
                    user.Email.Contains(rawSearch) ||
                    user.FullName.Contains(rawSearch) ||
                    (user.Department != null && user.Department.Contains(rawSearch)));
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(user => user.FullName)
                .ThenBy(user => user.Email)
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<User>(items, parameters.Page, parameters.PageSize, totalItems);
        }

        public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(user => user.UserRoles)!
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            return _context.Users
                .Include(user => user.UserRoles)!
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserSummaryData> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var baseQuery = _context.Users.AsNoTracking();

            var totalUsersTask = baseQuery.CountAsync(cancellationToken);
            var activeUsersTask = baseQuery.CountAsync(user => !user.IsDeleted && user.Status == "Active", cancellationToken);
            var lockedUsersTask = baseQuery.CountAsync(user => user.IsLocked && !user.IsDeleted, cancellationToken);
            var deletedUsersTask = baseQuery.CountAsync(user => user.IsDeleted, cancellationToken);

            var roleCountsTask = _context.UserRoles
                .AsNoTracking()
                .Where(ur => !ur.User.IsDeleted)
                .GroupBy(ur => ur.Role.Name)
                .Select(group => new { Role = group.Key, Count = group.Count() })
                .ToDictionaryAsync(group => group.Role, group => group.Count, cancellationToken);

            var departmentCountsTask = baseQuery
                .Where(user => !user.IsDeleted && user.Department != null && user.Department != string.Empty)
                .GroupBy(user => user.Department!)
                .Select(group => new { Department = group.Key, Count = group.Count() })
                .ToDictionaryAsync(group => group.Department, group => group.Count, cancellationToken);

            await Task.WhenAll(totalUsersTask, activeUsersTask, lockedUsersTask, deletedUsersTask, roleCountsTask, departmentCountsTask);

            return new UserSummaryData(
                totalUsersTask.Result,
                activeUsersTask.Result,
                lockedUsersTask.Result,
                deletedUsersTask.Result,
                roleCountsTask.Result,
                departmentCountsTask.Result);
        }
    }
}
