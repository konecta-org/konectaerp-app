using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos
{
    public class StockLevelResponseDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal ReorderQuantity { get; set; }
    }

    public class InventoryItemResponseDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int SafetyStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public decimal StandardCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalOnHand { get; set; }
        public decimal TotalReserved { get; set; }
        public decimal TotalAvailable { get; set; }
        public IEnumerable<StockLevelResponseDto> StockLevels { get; set; } = Array.Empty<StockLevelResponseDto>();
    }

    public class StockLevelUpsertDto
    {
        [Required]
        public int WarehouseId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal QuantityOnHand { get; set; }

        [Range(0, double.MaxValue)]
        public decimal QuantityReserved { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ReorderQuantity { get; set; }
    }

    public class InventoryItemUpsertDto
    {
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

        [Range(0, int.MaxValue)]
        public int SafetyStockLevel { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderPoint { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StandardCost { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public List<StockLevelUpsertDto> StockLevels { get; set; } = new();
    }
}
