using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string InvoiceNumber { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string CustomerName { get; set; } = default!;

        [MaxLength(128)]
        public string? CustomerEmail { get; set; }

        [MaxLength(128)]
        public string? CustomerContact { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [MaxLength(32)]
        public string Status { get; set; } = "Draft";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [MaxLength(16)]
        public string Currency { get; set; } = "USD";

        [MaxLength(256)]
        public string? Notes { get; set; }

        public ICollection<InvoiceLine>? Lines { get; set; }
    }
}
