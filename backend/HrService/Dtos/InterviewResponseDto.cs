using System;
using HrService.Models;

namespace HrService.Dtos
{
    public class InterviewResponseDto
    {
        public Guid Id { get; set; }
        public Guid JobApplicationId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public Guid? InterviewerEmployeeId { get; set; }
        public string? InterviewerName { get; set; }
        public DateTime ScheduledAt { get; set; }
        public InterviewMode Mode { get; set; }
        public string? Location { get; set; }
        public InterviewStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
