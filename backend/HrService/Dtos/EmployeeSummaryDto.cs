using System;
using HrService.Models;

namespace HrService.Dtos
{
    public class EmployeeSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string WorkEmail { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public EmploymentStatus Status { get; set; }
    }
}
