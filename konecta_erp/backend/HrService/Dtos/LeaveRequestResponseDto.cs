using System;
using HrService.Models;

namespace HrService.Dtos
{
    public class LeaveRequestResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public LeaveStatus Status { get; set; }
        public Guid? ApprovedByEmployeeId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
