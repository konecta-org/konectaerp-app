using System;
using HrService.Models;

namespace HrService.Dtos
{
    public class JobApplicationResponseDto
    {
        public Guid Id { get; set; }
        public Guid JobOpeningId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string? CandidatePhone { get; set; }
        public string? ResumeUrl { get; set; }
        public string? CoverLetter { get; set; }
        public JobApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
