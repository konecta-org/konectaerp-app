using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public class EmployeeAccountUpsertDto
    {
        [Required]
        [MaxLength(64)]
        public string EmployeeId { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string EmployeeName { get; set; } = default!;

        [MaxLength(64)]
        public string? EmployeeNumber { get; set; }

        [MaxLength(128)]
        public string? Department { get; set; }

        [MaxLength(128)]
        public string? JobTitle { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        [MaxLength(16)]
        public string Currency { get; set; } = "USD";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;

        [MaxLength(128)]
        public string? BankName { get; set; }

        [MaxLength(64)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(64)]
        public string? BankRoutingNumber { get; set; }

        [MaxLength(64)]
        public string? Iban { get; set; }
    }

    public class CompensationBonusCreateDto
    {
        [Required]
        [MaxLength(64)]
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

    public class CompensationDeductionCreateDto
    {
        [Required]
        [MaxLength(64)]
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

    public record EmployeeBonusResponseDto(
        int Id,
        string BonusType,
        decimal Amount,
        DateTime AwardedOn,
        string? Period,
        string? Reference,
        string? AwardedBy,
        string? Notes,
        string? SourceSystem);

    public record EmployeeDeductionResponseDto(
        int Id,
        string DeductionType,
        decimal Amount,
        DateTime AppliedOn,
        string? Period,
        string? Reference,
        string? AppliedBy,
        string? Notes,
        string? SourceSystem,
        bool IsRecurring);

    public record class EmployeeCompensationResponseDto
    {
        public string EmployeeId { get; init; } = string.Empty;
        public string EmployeeName { get; init; } = string.Empty;
        public string? EmployeeNumber { get; init; }
        public string? Department { get; init; }
        public string? JobTitle { get; init; }
        public decimal BaseSalary { get; init; }
        public string Currency { get; init; } = "USD";
        public DateTime EffectiveFrom { get; init; }
        public string? BankName { get; init; }
        public string? BankAccountNumber { get; init; }
        public string? BankRoutingNumber { get; init; }
        public string? Iban { get; init; }
        public decimal TotalBonusesYtd { get; init; }
        public decimal TotalDeductionsYtd { get; init; }
        public decimal NetCompensationYtd { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public IEnumerable<EmployeeBonusResponseDto> RecentBonuses { get; init; } = Array.Empty<EmployeeBonusResponseDto>();
        public IEnumerable<EmployeeDeductionResponseDto> RecentDeductions { get; init; } = Array.Empty<EmployeeDeductionResponseDto>();
    }

    public class EmployeeCompensationUpdateDto
    {
        [MaxLength(64)]
        public string? EmployeeNumber { get; set; }

        [MaxLength(128)]
        public string? Department { get; set; }

        [MaxLength(128)]
        public string? JobTitle { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        [MaxLength(16)]
        public string Currency { get; set; } = "USD";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;

        [MaxLength(128)]
        public string? BankName { get; set; }

        [MaxLength(64)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(64)]
        public string? BankRoutingNumber { get; set; }

        [MaxLength(64)]
        public string? Iban { get; set; }
    }
}
