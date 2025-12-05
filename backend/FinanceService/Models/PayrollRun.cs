using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class PayrollRun
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string PayrollNumber { get; set; } = default!;

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public DateTime PaymentDate { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Draft";

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGrossPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNetPay { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        public ICollection<PayrollEntry>? Entries { get; set; }
    }
}
