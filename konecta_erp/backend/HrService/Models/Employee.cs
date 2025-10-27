using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrService.Models
{
    public enum EmploymentStatus
    {
        Active,
        OnLeave,
        Resigned,
        Terminated
    }

    public class Employee
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string WorkEmail { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string PersonalEmail { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [Required]
        public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

        /// <summary>
        /// Identity user identifier provisioned by the AuthenticationService.
        /// Populated asynchronously after the account is created.
        /// </summary>
        public Guid? UserId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
