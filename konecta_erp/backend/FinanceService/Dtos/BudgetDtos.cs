using System.ComponentModel.DataAnnotations;

namespace FinanceService.Dtos
{
    public record BudgetLineResponseDto(
        int Id,
        string Category,
        decimal AllocatedAmount,
        decimal SpentAmount,
        string? Notes);

    public record BudgetResponseDto(
        int Id,
        string Name,
        string? Department,
        int FiscalYear,
        DateTime StartDate,
        DateTime EndDate,
        decimal TotalAmount,
        decimal SpentAmount,
        string? Notes,
        decimal RemainingAmount,
        IEnumerable<BudgetLineResponseDto> Lines);

    public class BudgetLineUpsertDto
    {
        [Required]
        [MaxLength(128)]
        public string Category { get; set; } = default!;

        [Range(0, double.MaxValue)]
        public decimal AllocatedAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SpentAmount { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }

    public class BudgetUpsertDto
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        [MaxLength(128)]
        public string? Department { get; set; }

        [Range(2000, 3000)]
        public int FiscalYear { get; set; } = DateTime.UtcNow.Year;

        public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 1, 1);

        public DateTime EndDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 12, 31);

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SpentAmount { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        public List<BudgetLineUpsertDto> Lines { get; set; } = new();
    }
}
