using AutoMapper;
using Microsoft.Extensions.Logging;
using UserManagementService.Dtos;
using UserManagementService.Models;
using UserManagementService.Repositories;

namespace UserManagementService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository repository,
            IRoleRepository roleRepository,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _repository = repository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<PagedResultDto<User>> GetUsersAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            return _repository.GetPagedAsync(parameters, cancellationToken);
        }

        public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User> CreateAsync(UserCreateDto dto, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
            var existing = await _repository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException($"User with email {dto.Email} already exists.");
            }

            var user = _mapper.Map<User>(dto);
            user.Id = string.IsNullOrWhiteSpace(dto.ExternalUserId) ? Guid.NewGuid().ToString() : dto.ExternalUserId;
            if (string.IsNullOrWhiteSpace(user.Status))
            {
                user.Status = "Active";
            }
            user.NormalizeEmail();
            user.IsDeleted = false;
            user.IsLocked = false;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            EnsureNameComponents(user);
            await ApplyRoleAssignmentsAsync(user, dto.RoleIds, dto.CreatedBy, cancellationToken);

            await _repository.AddAsync(user, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created user {UserId} ({Email})", user.Id, user.Email);
            return user;
        }

        public async Task<User?> UpdateAsync(string id, UserUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.UtcNow;
            user.DeactivatedAt = string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase)
                ? null
                : user.DeactivatedAt ?? DateTime.UtcNow;
            EnsureNameComponents(user);

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated user {UserId}", id);
            return user;
        }

        public async Task<bool> ChangeRoleAsync(string id, RoleChangeDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return false;
            }

            var targetRole = await _roleRepository.GetByNameAsync(dto.NewRole.Trim(), cancellationToken: cancellationToken);
            if (targetRole == null)
            {
                throw new InvalidOperationException($"Role '{dto.NewRole}' does not exist.");
            }

            await ApplyRoleAssignmentsAsync(user, new[] { targetRole.Id }, dto.ChangedBy, cancellationToken);

            user.UpdatedAt = DateTime.UtcNow;
            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Changed role for user {UserId} to {Role}", id, targetRole.Name);
            return true;
        }

        public async Task<bool> SetUserRolesAsync(string id, UserRoleAssignmentDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return false;
            }

            await ApplyRoleAssignmentsAsync(user, dto.RoleIds, dto.AssignedBy, cancellationToken);
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated roles for user {UserId}", id);
            return true;
        }

        public async Task<IReadOnlyCollection<RoleResponseDto>> GetUserRolesAsync(string id, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return Array.Empty<RoleResponseDto>();
            }

            var roles = user.UserRoles?
                .Select(ur => new RoleResponseDto(
                    ur.RoleId,
                    ur.Role.Name,
                    ur.Role.Description,
                    ur.Role.IsSystemDefault,
                    ur.Role.IsActive,
                    Enumerable.Empty<PermissionResponseDto>(),
                    0))
                .ToList() ?? new List<RoleResponseDto>();

            return roles;
        }

        public async Task<bool> UpdateStatusAsync(string id, UserStatusUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return false;
            }

            var status = dto.Status.Trim();
            user.Status = status;
            user.IsLocked = dto.LockAccount;
            user.UpdatedAt = DateTime.UtcNow;

            if (!user.IsDeleted && !string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                user.DeactivatedAt = DateTime.UtcNow;
            }

            if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                user.DeactivatedAt = null;
            }

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated status for user {UserId} to {Status}", id, status);
            return true;
        }

        public async Task<bool> SoftDeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return false;
            }

            user.IsDeleted = true;
            user.Status = "Deleted";
            user.DeactivatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Soft-deleted user {UserId}", id);
            return true;
        }

        public async Task<bool> RestoreAsync(string id, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null || !user.IsDeleted)
            {
                return false;
            }

            user.IsDeleted = false;
            user.Status = "Active";
            user.DeactivatedAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Restored user {UserId}", id);
            return true;
        }

        public async Task<bool> TerminateAsync(string id, string? reason, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return false;
            }

            user.IsDeleted = true;
            user.IsLocked = true;
            user.Status = "Terminated";
            user.DeactivatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Terminated user {UserId} ({Reason})", id, reason ?? "No reason supplied");
            return true;
        }

        public async Task<UserSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var summary = await _repository.GetSummaryAsync(cancellationToken);
            return new UserSummaryDto(
                summary.TotalUsers,
                summary.ActiveUsers,
                summary.LockedUsers,
                summary.DeletedUsers,
                summary.UsersPerRole,
                summary.UsersPerDepartment);
        }

        public async Task<User> CreateOrUpdateFromExternalAsync(string externalUserId, string email, string fullName, string role, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByIdAsync(externalUserId, cancellationToken);
            if (user != null)
            {
                UpdateExternalUser(user, email, fullName, role);
                await ApplyRoleAssignmentsAsync(user, await ResolveRoleIdsFromNameAsync(role, cancellationToken), "External", cancellationToken);
                _repository.Update(user);
                await _repository.SaveChangesAsync(cancellationToken);
                return user;
            }

            var normalizedEmail = email.Trim().ToUpperInvariant();
            var existing = await _repository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existing != null)
            {
                UpdateExternalUser(existing, email, fullName, role);
                await ApplyRoleAssignmentsAsync(existing, await ResolveRoleIdsFromNameAsync(role, cancellationToken), "External", cancellationToken);
                _repository.Update(existing);
                await _repository.SaveChangesAsync(cancellationToken);
                return existing;
            }

            var dto = new UserCreateDto
            {
                Email = email,
                FullName = fullName,
                RoleIds = await ResolveRoleIdsFromNameAsync(role, cancellationToken),
                Status = "Active",
                ExternalUserId = externalUserId
            };

            return await CreateAsync(dto, cancellationToken);
        }

        private static void UpdateExternalUser(User user, string email, string fullName, string role)
        {
            user.Email = email.Trim();
            user.NormalizeEmail();
            user.FullName = fullName;
            user.IsDeleted = false;
            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;
            EnsureNameComponents(user);
        }

        private async Task ApplyRoleAssignmentsAsync(User user, IEnumerable<int> roleIds, string? assignedBy, CancellationToken cancellationToken)
        {
            user.UserRoles ??= new List<UserRole>();
            var distinctRoleIds = roleIds?.Distinct().ToList() ?? new List<int>();

            if (distinctRoleIds.Count == 0)
            {
                // Fallback to first system default role if exists
                var defaultRole = (await _roleRepository.GetAllAsync(false, cancellationToken))
                    .FirstOrDefault(r => r.IsSystemDefault && r.IsActive);

                if (defaultRole != null)
                {
                    distinctRoleIds.Add(defaultRole.Id);
                }
            }

            if (distinctRoleIds.Count == 0)
            {
                user.UserRoles.Clear();
                return;
            }

            var roles = await _roleRepository.GetByIdsAsync(distinctRoleIds, false, cancellationToken);
            if (roles.Count != distinctRoleIds.Count)
            {
                var missing = distinctRoleIds.Except(roles.Select(r => r.Id)).ToArray();
                throw new InvalidOperationException($"One or more roles do not exist: {string.Join(", ", missing)}");
            }

            var existingAssignments = user.UserRoles.ToList();
            var toRemove = existingAssignments.Where(ur => !distinctRoleIds.Contains(ur.RoleId)).ToList();
            foreach (var remove in toRemove)
            {
                user.UserRoles.Remove(remove);
            }

            var existingIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();
            foreach (var role in roles)
            {
                if (!existingIds.Contains(role.Id))
                {
                    user.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = assignedBy
                    });
                }
            }
        }

        private async Task<List<int>> ResolveRoleIdsFromNameAsync(string roleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return new List<int>();
            }

            var role = await _roleRepository.GetByNameAsync(roleName.Trim(), cancellationToken: cancellationToken);
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' does not exist.");
            }

            return new List<int> { role.Id };
        }

        private static void EnsureNameComponents(User user)
        {
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                return;
            }

            var parts = user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = parts[0];
            }

            if (string.IsNullOrWhiteSpace(user.LastName) && parts.Length > 1)
            {
                user.LastName = string.Join(' ', parts.Skip(1));
            }
        }
    }
}
