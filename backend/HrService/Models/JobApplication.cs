using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public enum JobApplicationStatus
    {
        Submitted,
        UnderReview,
        Interviewing,
        Offered,
        Rejected,
        Hired,
        Withdrawn
    }

    public class JobApplication
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobOpeningId { get; set; }

        public JobOpening? JobOpening { get; set; }

        [Required, MaxLength(150)]
        public string CandidateName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string CandidateEmail { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? CandidatePhone { get; set; }

        [MaxLength(2048)]
        public string? ResumeUrl { get; set; }

        [MaxLength(4000)]
        public string? CoverLetter { get; set; }

        [Required]
        public JobApplicationStatus Status { get; set; } = JobApplicationStatus.Submitted;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Interview>? Interviews { get; set; }
    }
}
