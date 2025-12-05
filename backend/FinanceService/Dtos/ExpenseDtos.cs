using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public record ExpenseResponseDto(
        int Id,
        string ExpenseNumber,
        string Category,
        string? Vendor,
        string Description,
        DateTime IncurredOn,
        string Status,
        string PaymentMethod,
        decimal Amount,
        string? Notes);

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
