using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public class InvoiceLineResponseDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public string CustomerName { get; set; } = default!;
        public string? CustomerEmail { get; set; }
        public string? CustomerContact { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = default!;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Notes { get; set; }
        public IEnumerable<InvoiceLineResponseDto> Lines { get; set; } = new List<InvoiceLineResponseDto>();
    }

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
