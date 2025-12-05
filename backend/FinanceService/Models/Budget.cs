using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        [MaxLength(128)]
        public string? Department { get; set; }

        public int FiscalYear { get; set; } = DateTime.UtcNow.Year;

        public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 1, 1);

        public DateTime EndDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 12, 31);

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpentAmount { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        public ICollection<BudgetLine>? Lines { get; set; }
    }
}
