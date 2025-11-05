using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public bool IsSystemDefault { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RolePermission>? Permissions { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
