using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Models
{
    public class StockLevel
    {
        public int InventoryItemId { get; set; }
        public int WarehouseId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityOnHand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityReserved { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReorderQuantity { get; set; }

        public InventoryItem? InventoryItem { get; set; }

        public Warehouse? Warehouse { get; set; }
    }
}
