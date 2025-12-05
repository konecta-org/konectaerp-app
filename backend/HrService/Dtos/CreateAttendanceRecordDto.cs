using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class CreateAttendanceRecordDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
