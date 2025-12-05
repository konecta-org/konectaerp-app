using InventoryService.Models;

namespace InventoryService.Repositories
{
    public interface IStockTransactionRepository
    {
        Task<IEnumerable<StockTransaction>> GetRecentAsync(int page = 1, int pageSize = 50, int? itemId = null, int? warehouseId = null, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<StockTransaction> transactions, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
