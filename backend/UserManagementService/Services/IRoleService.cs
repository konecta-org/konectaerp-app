using UserManagementService.Dtos;

namespace UserManagementService.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponseDto>> GetRolesAsync(CancellationToken cancellationToken = default);
        Task<RoleResponseDto?> GetRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto request, CancellationToken cancellationToken = default);
        Task<RoleResponseDto?> UpdateRoleAsync(int roleId, RoleUpdateDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
        Task<RoleResponseDto?> UpdateRolePermissionsAsync(int roleId, RolePermissionsUpdateDto request, CancellationToken cancellationToken = default);

        Task<IEnumerable<PermissionResponseDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);
        Task<PermissionResponseDto?> GetPermissionAsync(int permissionId, CancellationToken cancellationToken = default);
        Task<PermissionResponseDto> CreatePermissionAsync(PermissionCreateDto request, CancellationToken cancellationToken = default);
        Task<PermissionResponseDto?> UpdatePermissionAsync(int permissionId, PermissionUpdateDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default);
    }
}
