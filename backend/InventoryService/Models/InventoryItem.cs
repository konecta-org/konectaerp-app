using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Sku { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        [MaxLength(256)]
        public string? Description { get; set; }

        [MaxLength(64)]
        public string? Category { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Active";

        [MaxLength(32)]
        public string UnitOfMeasure { get; set; } = "Each";

        public int SafetyStockLevel { get; set; } = 0;

        public int ReorderPoint { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal StandardCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public ICollection<StockLevel>? StockLevels { get; set; }

        public ICollection<StockTransaction>? Transactions { get; set; }
    }
}
