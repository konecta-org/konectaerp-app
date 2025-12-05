using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class UpdateLeaveRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [MaxLength(2000)]
        public string? Reason { get; set; }

        public LeaveStatus Status { get; set; }

        public Guid? ApprovedByEmployeeId { get; set; }
    }
}
