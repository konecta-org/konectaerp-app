using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class RoleChangeDto
    {
        [Required]
        [MaxLength(64)]
        public string NewRole { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? ChangedBy { get; set; }
    }
}
