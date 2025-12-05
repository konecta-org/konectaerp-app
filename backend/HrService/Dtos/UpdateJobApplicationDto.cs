using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class UpdateJobApplicationDto
    {
        [Required]
        public Guid Id { get; set; }

        public JobApplicationStatus Status { get; set; }

        [MaxLength(4000)]
        public string? CoverLetter { get; set; }
    }
}
