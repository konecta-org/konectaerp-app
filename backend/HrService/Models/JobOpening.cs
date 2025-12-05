using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public enum EmploymentType
    {
        FullTime,
        PartTime,
        Contract,
        Internship,
        Temporary
    }

    public enum JobStatus
    {
        Draft,
        Open,
        Closed,
        Filled,
        Cancelled
    }

    public class JobOpening
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [MaxLength(4000)]
        public string? Requirements { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [Required]
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Draft;

        [Required]
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ClosingDate { get; set; }

        public Guid? DepartmentId { get; set; }

        public Department? Department { get; set; }

        public ICollection<JobApplication>? Applications { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
