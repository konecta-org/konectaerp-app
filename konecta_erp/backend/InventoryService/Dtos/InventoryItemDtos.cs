using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos
{
    public record StockLevelResponseDto(
        int WarehouseId,
        string WarehouseCode,
        string WarehouseName,
        decimal QuantityOnHand,
        decimal QuantityReserved,
        decimal AvailableQuantity,
        decimal ReorderQuantity);

    public record InventoryItemResponseDto(
        int Id,
        string Sku,
        string Name,
        string? Description,
        string? Category,
        string Status,
        string UnitOfMeasure,
        int SafetyStockLevel,
        int ReorderPoint,
        decimal StandardCost,
        decimal UnitPrice,
        decimal TotalOnHand,
        decimal TotalReserved,
        decimal TotalAvailable,
        IEnumerable<StockLevelResponseDto> StockLevels);

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
