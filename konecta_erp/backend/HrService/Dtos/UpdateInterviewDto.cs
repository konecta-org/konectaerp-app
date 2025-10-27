using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class UpdateInterviewDto
    {
        [Required]
        public Guid Id { get; set; }

        public Guid? InterviewerEmployeeId { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public InterviewMode Mode { get; set; }

        [MaxLength(250)]
        public string? Location { get; set; }

        public InterviewStatus Status { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }
    }
}
