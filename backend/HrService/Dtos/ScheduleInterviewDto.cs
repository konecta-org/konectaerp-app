using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class ScheduleInterviewDto
    {
        [Required]
        public Guid JobApplicationId { get; set; }

        public Guid? InterviewerEmployeeId { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public InterviewMode Mode { get; set; } = InterviewMode.InPerson;

        [MaxLength(250)]
        public string? Location { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }
    }
}
