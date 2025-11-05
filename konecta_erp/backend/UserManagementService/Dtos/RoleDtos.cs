using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class RoleCreateDto
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public bool IsSystemDefault { get; set; }
    }

    public class RoleUpdateDto
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RolePermissionsUpdateDto
    {
        public List<int> PermissionIds { get; set; } = new();
    }

    public record RoleResponseDto(
        int Id,
        string Name,
        string? Description,
        bool IsSystemDefault,
        bool IsActive,
        IEnumerable<PermissionResponseDto> Permissions,
        int AssignedUsers);

    public record PermissionResponseDto(
        int Id,
        string Name,
        string Category,
        string? Description);

    public class PermissionCreateDto
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(64)]
        public string Category { get; set; } = "General";

        [MaxLength(256)]
        public string? Description { get; set; }
    }

    public class PermissionUpdateDto
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(64)]
        public string Category { get; set; } = "General";

        [MaxLength(256)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UserRoleAssignmentDto
    {
        public List<int> RoleIds { get; set; } = new();

        [MaxLength(128)]
        public string? AssignedBy { get; set; }
    }
}
