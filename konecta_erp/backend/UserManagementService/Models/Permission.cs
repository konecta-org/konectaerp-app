using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        [MaxLength(64)]
        public string Category { get; set; } = "General";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RolePermission>? RoleAssignments { get; set; }
    }
}
