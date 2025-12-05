using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    public class StockTransactionRepository : IStockTransactionRepository
    {
        private readonly AppDbContext _context;

        public StockTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StockTransaction>> GetRecentAsync(int page = 1, int pageSize = 50, int? itemId = null, int? warehouseId = null, CancellationToken cancellationToken = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            IQueryable<StockTransaction> query = _context.StockTransactions
                .Include(tx => tx.InventoryItem)
                .Include(tx => tx.Warehouse);

            if (itemId.HasValue)
            {
                query = query.Where(tx => tx.InventoryItemId == itemId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(tx => tx.WarehouseId == warehouseId.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(tx => tx.OccurredAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<StockTransaction> transactions, CancellationToken cancellationToken = default)
        {
            await _context.StockTransactions.AddRangeAsync(transactions, cancellationToken);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
