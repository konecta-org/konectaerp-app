using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public record InvoiceLineResponseDto(
        int Id,
        string ItemCode,
        string Description,
        decimal Quantity,
        decimal UnitPrice,
        decimal LineTotal);

    public record InvoiceResponseDto(
        int Id,
        string InvoiceNumber,
        string CustomerName,
        string? CustomerEmail,
        string? CustomerContact,
        DateTime IssueDate,
        DateTime DueDate,
        string Status,
        decimal Subtotal,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal PaidAmount,
        decimal BalanceDue,
        string Currency,
        string? Notes,
        IEnumerable<InvoiceLineResponseDto> Lines);

    public class InvoiceLineUpsertDto
    {
        [MaxLength(128)]
        public string? ItemCode { get; set; }

        [Required]
        [MaxLength(256)]
        public string Description { get; set; } = default!;

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class InvoiceUpsertDto
    {
        [Required]
        [MaxLength(64)]
        public string InvoiceNumber { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string CustomerName { get; set; } = default!;

        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [MaxLength(128)]
        public string? CustomerContact { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [MaxLength(32)]
        public string Status { get; set; } = "Draft";

        [Range(0, double.MaxValue)]
        public decimal Subtotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }

        [Required]
        [MaxLength(16)]
        public string Currency { get; set; } = "USD";

        [MaxLength(256)]
        public string? Notes { get; set; }

        public List<InvoiceLineUpsertDto> Lines { get; set; } = new();
    }
}
