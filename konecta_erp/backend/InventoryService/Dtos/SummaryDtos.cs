namespace InventoryService.Dtos
{
    public record InventorySummaryDto(
        int TotalActiveItems,
        int TotalWarehouses,
        decimal TotalQuantityOnHand,
        decimal TotalQuantityReserved,
        int ItemsBelowSafetyStock);
}
