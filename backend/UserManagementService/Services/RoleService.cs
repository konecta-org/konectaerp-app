using AutoMapper;
using Microsoft.Extensions.Logging;
using UserManagementService.Dtos;
using UserManagementService.Models;
using UserManagementService.Repositories;

namespace UserManagementService.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IMapper mapper,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<RoleResponseDto>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _roleRepository.GetAllAsync(includePermissions: true, cancellationToken);
            return roles.Select(MapRole);
        }

        public async Task<RoleResponseDto?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, includePermissions: true, cancellationToken);
            return role == null ? null : MapRole(role);
        }

        public async Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto request, CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();
            if (await _roleRepository.NameExistsAsync(name, null, cancellationToken))
            {
                throw new InvalidOperationException($"Role '{name}' already exists.");
            }

            var role = new Role
            {
                Name = name,
                Description = request.Description?.Trim(),
                IsSystemDefault = request.IsSystemDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _roleRepository.AddAsync(role, cancellationToken);
            await _roleRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created role {RoleName}", role.Name);

            return MapRole(role);
        }

        public async Task<RoleResponseDto?> UpdateRoleAsync(int roleId, RoleUpdateDto request, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, includePermissions: true, cancellationToken);
            if (role == null)
            {
                return null;
            }

            var name = request.Name.Trim();
            if (!string.Equals(role.Name, name, StringComparison.OrdinalIgnoreCase) &&
                await _roleRepository.NameExistsAsync(name, roleId, cancellationToken))
            {
                throw new InvalidOperationException($"Role '{name}' already exists.");
            }

            role.Name = name;
            role.Description = request.Description?.Trim();
            role.IsActive = request.IsActive;
            role.UpdatedAt = DateTime.UtcNow;

            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated role {RoleId}", roleId);

            return MapRole(role);
        }

        public async Task<bool> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, includePermissions: false, cancellationToken);
            if (role == null)
            {
                return false;
            }

            if (role.IsSystemDefault)
            {
                throw new InvalidOperationException("System roles cannot be deleted.");
            }

            _roleRepository.Remove(role);
            await _roleRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deleted role {RoleId}", roleId);
            return true;
        }

        public async Task<RoleResponseDto?> UpdateRolePermissionsAsync(int roleId, RolePermissionsUpdateDto request, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, includePermissions: true, cancellationToken);
            if (role == null)
            {
                return null;
            }

            var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
            var validPermissionIds = permissions.Select(p => p.Id).ToHashSet();

            var requested = request.PermissionIds.Distinct().ToList();

            foreach (var pid in requested)
            {
                if (!validPermissionIds.Contains(pid))
                {
                    throw new InvalidOperationException($"Permission '{pid}' does not exist.");
                }
            }

            role.Permissions ??= new List<RolePermission>();

            // Remove unselected
            var toRemove = role.Permissions.Where(rp => !requested.Contains(rp.PermissionId)).ToList();
            foreach (var remove in toRemove)
            {
                role.Permissions.Remove(remove);
            }

            // Add new
            var existingIds = role.Permissions.Select(rp => rp.PermissionId).ToHashSet();
            foreach (var pid in requested)
            {
                if (!existingIds.Contains(pid))
                {
                    role.Permissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = pid,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }

            role.UpdatedAt = DateTime.UtcNow;
            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated permissions for role {RoleId}", roleId);

            return MapRole(role);
        }

        public async Task<IEnumerable<PermissionResponseDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
        {
            var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
            return permissions.Select(_mapper.Map<PermissionResponseDto>);
        }

        public async Task<PermissionResponseDto?> GetPermissionAsync(int permissionId, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            return permission == null ? null : _mapper.Map<PermissionResponseDto>(permission);
        }

        public async Task<PermissionResponseDto> CreatePermissionAsync(PermissionCreateDto request, CancellationToken cancellationToken = default)
        {
            var name = request.Name.Trim();
            if (await _permissionRepository.NameExistsAsync(name, null, cancellationToken))
            {
                throw new InvalidOperationException($"Permission '{name}' already exists.");
            }

            var permission = new Permission
            {
                Name = name,
                Category = request.Category.Trim(),
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _permissionRepository.AddAsync(permission, cancellationToken);
            await _permissionRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created permission {Permission}", permission.Name);

            return _mapper.Map<PermissionResponseDto>(permission);
        }

        public async Task<PermissionResponseDto?> UpdatePermissionAsync(int permissionId, PermissionUpdateDto request, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
            {
                return null;
            }

            var name = request.Name.Trim();
            if (!string.Equals(permission.Name, name, StringComparison.OrdinalIgnoreCase) &&
                await _permissionRepository.NameExistsAsync(name, permissionId, cancellationToken))
            {
                throw new InvalidOperationException($"Permission '{name}' already exists.");
            }

            permission.Name = name;
            permission.Category = request.Category.Trim();
            permission.Description = request.Description?.Trim();
            permission.IsActive = request.IsActive;
            permission.UpdatedAt = DateTime.UtcNow;

            _permissionRepository.Update(permission);
            await _permissionRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated permission {PermissionId}", permissionId);

            return _mapper.Map<PermissionResponseDto>(permission);
        }

        public async Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
            {
                return false;
            }

            _permissionRepository.Remove(permission);
            await _permissionRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deleted permission {PermissionId}", permissionId);
            return true;
        }

        private RoleResponseDto MapRole(Role role)
        {
            var permissions = role.Permissions?.Select(rp => _mapper.Map<PermissionResponseDto>(rp.Permission)) ??
                              Enumerable.Empty<PermissionResponseDto>();
            var assignedUsers = role.UserRoles?.Count ?? 0;

            return new RoleResponseDto(
                role.Id,
                role.Name,
                role.Description,
                role.IsSystemDefault,
                role.IsActive,
                permissions,
                assignedUsers);
        }
    }
}
