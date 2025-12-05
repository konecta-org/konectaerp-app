using System;
using HrService.Models;

namespace HrService.Dtos
{
    public class ResignationRequestResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? Reason { get; set; }
        public ResignationStatus Status { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNotes { get; set; }
        public Guid? ApprovedByEmployeeId { get; set; }
        public string? ApprovedByName { get; set; }
        public bool? EligibleForRehire { get; set; }
    }
}
