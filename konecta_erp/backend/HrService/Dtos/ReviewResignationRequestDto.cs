using System;
using System.ComponentModel.DataAnnotations;
using HrService.Models;

namespace HrService.Dtos
{
    public class ReviewResignationRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public ResignationStatus Decision { get; set; }

        [MaxLength(2000)]
        public string? DecisionNotes { get; set; }

        public Guid? ApprovedByEmployeeId { get; set; }

        public bool? EligibleForRehire { get; set; }
    }
}
