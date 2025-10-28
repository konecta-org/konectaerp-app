using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public enum ResignationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class ResignationRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EffectiveDate { get; set; }

        [MaxLength(2000)]
        public string? Reason { get; set; }

        [Required]
        public ResignationStatus Status { get; set; } = ResignationStatus.Pending;

        public DateTime? DecidedAt { get; set; }

        [MaxLength(2000)]
        public string? DecisionNotes { get; set; }

        public bool? EligibleForRehire { get; set; }

        public Guid? ApprovedByEmployeeId { get; set; }

        public Employee? ApprovedBy { get; set; }
    }
}
