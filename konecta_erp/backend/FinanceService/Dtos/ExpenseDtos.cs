using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public class ExpenseResponseDto
    {
        public int Id { get; set; }
        public string ExpenseNumber { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string? Vendor { get; set; }
        public string Description { get; set; } = default!;
        public DateTime IncurredOn { get; set; }
        public string Status { get; set; } = default!;
        public string PaymentMethod { get; set; } = default!;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class ExpenseUpsertDto
    {
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

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }
}
