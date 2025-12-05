using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class UserUpdateDto
    {
        [Required]
        [MaxLength(128)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? FirstName { get; set; }

        [MaxLength(64)]
        public string? LastName { get; set; }

        [MaxLength(128)]
        public string? Department { get; set; }

        [MaxLength(128)]
        public string? JobTitle { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Active";

        [MaxLength(32)]
        public string? PhoneNumber { get; set; }

        [MaxLength(64)]
        public string? ManagerId { get; set; }

        public bool IsLocked { get; set; }
    }
}
