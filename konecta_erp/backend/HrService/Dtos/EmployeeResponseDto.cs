using System;
using HrService.Models;

namespace HrService.Dtos
{
    public class EmployeeResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string WorkEmail { get; set; } = string.Empty;
        public string PersonalEmail { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public EmploymentStatus Status { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExitDate { get; set; }
        public string? ExitReason { get; set; }
        public bool? EligibleForRehire { get; set; }
    }
}
