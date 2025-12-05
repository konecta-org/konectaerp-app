using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public enum LeaveType
    {
        Vacation,
        Sick,
        Unpaid,
        Parental,
        Bereavement,
        Other
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public class LeaveRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.Vacation;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(2000)]
        public string? Reason { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public Guid? ApprovedByEmployeeId { get; set; }

        public Employee? ApprovedBy { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
