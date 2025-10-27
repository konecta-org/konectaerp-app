using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models
{
    public class KonectaUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string WorkEmail { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Roles { get; set; } = string.Empty;

        public DateTime ProvisionedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
