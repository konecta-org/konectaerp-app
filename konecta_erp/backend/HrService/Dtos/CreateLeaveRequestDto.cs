using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class CreateLeaveRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        public LeaveType LeaveType { get; set; } = LeaveType.Vacation;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(2000)]
        public string? Reason { get; set; }
    }
}
