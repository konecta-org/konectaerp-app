using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public enum InterviewStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        NoShow
    }

    public enum InterviewMode
    {
        InPerson,
        Virtual,
        Phone
    }

    public class Interview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobApplicationId { get; set; }

        public JobApplication? JobApplication { get; set; }

        public Guid? InterviewerEmployeeId { get; set; }

        public Employee? Interviewer { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Required]
        public InterviewMode Mode { get; set; } = InterviewMode.InPerson;

        [MaxLength(250)]
        public string? Location { get; set; }

        [Required]
        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        [MaxLength(4000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
