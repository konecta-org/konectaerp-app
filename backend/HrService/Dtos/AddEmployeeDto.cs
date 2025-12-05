using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class AddEmployeeDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string WorkEmail { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string PersonalEmail { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [Phone]
        [MaxLength(32)]
        public string? PhoneNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        public DateTime? HireDate { get; set; }

        public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

        [Required]
        public Guid DepartmentId { get; set; }
    }
}
