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
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, IMapper mapper, ILogger<UserService> logger)
        {
            _repository = repository;
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
            if (string.IsNullOrWhiteSpace(user.Role))
            {
                user.Role = "Employee";
            }
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

            user.Role = dto.NewRole.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            _repository.Update(user);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Changed role for user {UserId} to {Role}", id, dto.NewRole);
            return true;
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
                _repository.Update(user);
                await _repository.SaveChangesAsync(cancellationToken);
                return user;
            }

            var normalizedEmail = email.Trim().ToUpperInvariant();
            var existing = await _repository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existing != null)
            {
                UpdateExternalUser(existing, email, fullName, role);
                existing.Id = existing.Id; // maintain existing id
                _repository.Update(existing);
                await _repository.SaveChangesAsync(cancellationToken);
                return existing;
            }

            var dto = new UserCreateDto
            {
                Email = email,
                FullName = fullName,
                Role = string.IsNullOrWhiteSpace(role) ? "Employee" : role,
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
            user.Role = string.IsNullOrWhiteSpace(role) ? user.Role : role.Trim();
            user.IsDeleted = false;
            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;
            EnsureNameComponents(user);
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
