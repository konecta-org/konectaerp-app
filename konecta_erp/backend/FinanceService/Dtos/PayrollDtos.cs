using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public class PayrollEntryResponseDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = default!;
        public string EmployeeName { get; set; } = default!;
        public decimal GrossPay { get; set; }
        public decimal NetPay { get; set; }
        public decimal Deductions { get; set; }
        public decimal Taxes { get; set; }
        public string? Notes { get; set; }
    }

    public class PayrollRunResponseDto
    {
        public int Id { get; set; }
        public string PayrollNumber { get; set; } = default!;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = default!;
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public string? Notes { get; set; }
        public IEnumerable<PayrollEntryResponseDto> Entries { get; set; } = new List<PayrollEntryResponseDto>();
    }

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
