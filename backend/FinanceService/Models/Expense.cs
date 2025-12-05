using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string ExpenseNumber { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string Category { get; set; } = default!;

        [MaxLength(256)]
        public string? Vendor { get; set; }

        [Required]
        [MaxLength(512)]
        public string Description { get; set; } = default!;

        public DateTime IncurredOn { get; set; } = DateTime.UtcNow;

        [MaxLength(32)]
        public string Status { get; set; } = "Pending";

        [MaxLength(64)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }
}
