using InventoryService.Data;
using InventoryService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services
{
    public class InventorySummaryService : IInventorySummaryService
    {
        private readonly AppDbContext _context;

        public InventorySummaryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventorySummaryDto> BuildSummaryAsync(CancellationToken cancellationToken = default)
        {
            var totalActiveItems = await _context.InventoryItems
                .CountAsync(item => item.Status == "Active", cancellationToken);

            var totalWarehouses = await _context.Warehouses.CountAsync(cancellationToken);

            var stockTotals = await _context.StockLevels
                .GroupBy(level => 1)
                .Select(group => new
                {
                    OnHand = group.Sum(level => level.QuantityOnHand),
                    Reserved = group.Sum(level => level.QuantityReserved)
                })
                .FirstOrDefaultAsync(cancellationToken) ?? new { OnHand = 0m, Reserved = 0m };

            var itemsBelowSafetyStock = await _context.InventoryItems
                .Select(item => new
                {
                    ItemId = item.Id,
                    Required = item.SafetyStockLevel,
                    OnHand = item.StockLevels.Sum(level => level.QuantityOnHand)
                })
                .CountAsync(data => data.OnHand < data.Required, cancellationToken);

            return new InventorySummaryDto(
                totalActiveItems,
                totalWarehouses,
                Round(stockTotals.OnHand),
                Round(stockTotals.Reserved),
                itemsBelowSafetyStock);
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
