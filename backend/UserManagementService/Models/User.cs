using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementService.Models
{
    public class User
    {
        [Key]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string NormalizedEmail { get; set; } = string.Empty;

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

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<UserRole>? UserRoles { get; set; }

        public void NormalizeEmail()
        {
            NormalizedEmail = Email.ToUpperInvariant();
        }
    }
}
