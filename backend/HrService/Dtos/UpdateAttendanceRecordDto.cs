using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class UpdateAttendanceRecordDto
    {
        [Required]
        public Guid Id { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public AttendanceStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
