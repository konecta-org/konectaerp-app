using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos
{
    public class IssueEmployeeBonusesDto
    {
        [Required]
        public ICollection<EmployeeBonusCommandItemDto> Bonuses { get; set; } = new List<EmployeeBonusCommandItemDto>();

        [MaxLength(128)]
        public string? IssuedBy { get; set; }
    }

    public class EmployeeBonusCommandItemDto
    {
        [Required, MaxLength(64)]
        public string BonusType { get; set; } = "General";

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime AwardedOn { get; set; } = DateTime.UtcNow;

        [MaxLength(32)]
        public string? Period { get; set; }

        [MaxLength(256)]
        public string? Reference { get; set; }

        [MaxLength(128)]
        public string? AwardedBy { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        [MaxLength(64)]
        public string? SourceSystem { get; set; }
    }

    public class IssueEmployeeDeductionsDto
    {
        [Required]
        public ICollection<EmployeeDeductionCommandItemDto> Deductions { get; set; } = new List<EmployeeDeductionCommandItemDto>();

        [MaxLength(128)]
        public string? IssuedBy { get; set; }
    }

    public class EmployeeDeductionCommandItemDto
    {
        [Required, MaxLength(64)]
        public string DeductionType { get; set; } = "General";

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        [MaxLength(32)]
        public string? Period { get; set; }

        [MaxLength(256)]
        public string? Reference { get; set; }

        [MaxLength(128)]
        public string? AppliedBy { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        [MaxLength(64)]
        public string? SourceSystem { get; set; }

        public bool IsRecurring { get; set; }
    }
}
