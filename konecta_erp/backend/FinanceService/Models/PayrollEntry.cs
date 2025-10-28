using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class PayrollEntry
    {
        public int Id { get; set; }

        public int PayrollRunId { get; set; }

        [Required]
        [MaxLength(128)]
        public string EmployeeId { get; set; } = default!;

        [Required]
        [MaxLength(256)]
        public string EmployeeName { get; set; } = default!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Taxes { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        public PayrollRun? PayrollRun { get; set; }
    }
}
