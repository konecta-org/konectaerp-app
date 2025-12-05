using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Remote,
        OnLeave,
        Holiday
    }

    public class AttendanceRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
