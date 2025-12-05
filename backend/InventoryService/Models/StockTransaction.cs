using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }

        public int InventoryItemId { get; set; }

        public int WarehouseId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(32)]
        public string TransactionType { get; set; } = default!;

        [MaxLength(64)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(128)]
        public string? PerformedBy { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(256)]
        public string? Notes { get; set; }

        public InventoryItem? InventoryItem { get; set; }

        public Warehouse? Warehouse { get; set; }
    }
}
