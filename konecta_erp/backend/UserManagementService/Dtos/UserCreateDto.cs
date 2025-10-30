using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class UserCreateDto
    {
        [EmailAddress]
        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

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

        [MaxLength(64)]
        public string Role { get; set; } = "Employee";

        [MaxLength(32)]
        public string Status { get; set; } = "Active";

        [MaxLength(32)]
        public string? PhoneNumber { get; set; }

        [MaxLength(64)]
        public string? ManagerId { get; set; }

        [MaxLength(64)]
        public string? ExternalUserId { get; set; }

        [MaxLength(256)]
        public string? CreatedBy { get; set; }
    }
}
