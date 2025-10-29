using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos
{
    public class StockAdjustmentRequestDto
    {
        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(32)]
        public string TransactionType { get; set; } = default!;

        public bool Increase { get; set; } = true;

        [MaxLength(64)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(128)]
        public string? PerformedBy { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }

    public class StockTransferRequestDto
    {
        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public int FromWarehouseId { get; set; }

        [Required]
        public int ToWarehouseId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [MaxLength(64)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(128)]
        public string? PerformedBy { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }

    public record StockTransactionResponseDto(
        int Id,
        int InventoryItemId,
        string ItemSku,
        string ItemName,
        int WarehouseId,
        string WarehouseCode,
        string WarehouseName,
        decimal Quantity,
        string TransactionType,
        string? ReferenceNumber,
        string? PerformedBy,
        DateTime OccurredAtUtc,
        string? Notes);
}
