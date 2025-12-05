using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class BudgetLine
    {
        public int Id { get; set; }

        public int BudgetId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Category { get; set; } = default!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpentAmount { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        public Budget? Budget { get; set; }
    }
}
