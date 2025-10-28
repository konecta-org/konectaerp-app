using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public record PayrollEntryResponseDto(
        int Id,
        string EmployeeId,
        string EmployeeName,
        decimal GrossPay,
        decimal NetPay,
        decimal Deductions,
        decimal Taxes,
        string? Notes);

    public record PayrollRunResponseDto(
        int Id,
        string PayrollNumber,
        DateTime PeriodStart,
        DateTime PeriodEnd,
        DateTime PaymentDate,
        string Status,
        decimal TotalGrossPay,
        decimal TotalNetPay,
        string? Notes,
        IEnumerable<PayrollEntryResponseDto> Entries);

    public class PayrollEntryUpsertDto
    {
        [Required]
        [MaxLength(128)]
        public string EmployeeId { get; set; } = default!;

        [Required]
        [MaxLength(256)]
        public string EmployeeName { get; set; } = default!;

        [Range(0, double.MaxValue)]
        public decimal GrossPay { get; set; }

        [Range(0, double.MaxValue)]
        public decimal NetPay { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Deductions { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Taxes { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }

    public class PayrollRunUpsertDto
    {
        [Required]
        [MaxLength(64)]
        public string PayrollNumber { get; set; } = default!;

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public DateTime PaymentDate { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Draft";

        [Range(0, double.MaxValue)]
        public decimal TotalGrossPay { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalNetPay { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        public List<PayrollEntryUpsertDto> Entries { get; set; } = new();
    }
}
