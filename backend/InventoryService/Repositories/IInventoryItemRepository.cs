using InventoryService.Models;

namespace InventoryService.Repositories
{
    public interface IInventoryItemRepository
    {
        Task<IEnumerable<InventoryItem>> GetAllAsync(string? category = null, bool includeStock = true, CancellationToken cancellationToken = default);
        Task<InventoryItem?> GetByIdAsync(int id, bool includeStock = true, CancellationToken cancellationToken = default);
        Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
        Task<bool> SkuExistsAsync(string sku, int? excludeItemId = null, CancellationToken cancellationToken = default);
        Task UpsertStockLevelsAsync(InventoryItem item, IEnumerable<StockLevel> stockLevels, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
