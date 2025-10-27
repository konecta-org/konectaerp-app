using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class UpdateJobOpeningDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [MaxLength(4000)]
        public string? Requirements { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public EmploymentType EmploymentType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalaryMin { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalaryMax { get; set; }

        public Guid? DepartmentId { get; set; }

        public DateTime? ClosingDate { get; set; }

        public JobStatus Status { get; set; }
    }
}
