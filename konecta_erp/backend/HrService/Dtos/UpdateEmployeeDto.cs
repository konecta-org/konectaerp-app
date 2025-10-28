using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class UpdateEmployeeDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string WorkEmail { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string PersonalEmail { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public EmploymentStatus Status { get; set; }

        public DateTime? HireDate { get; set; }

        public Guid? UserId { get; set; }

        public DateTime? ExitDate { get; set; }

        [MaxLength(2000)]
        public string? ExitReason { get; set; }

        public bool? EligibleForRehire { get; set; }
    }
}
