using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos
{
    public class CreateJobApplicationDto
    {
        [Required]
        public Guid JobOpeningId { get; set; }

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
    }
}
