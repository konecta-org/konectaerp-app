using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    public class InventoryItemRepository : IInventoryItemRepository
    {
        private readonly AppDbContext _context;

        public InventoryItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryItem>> GetAllAsync(string? category = null, bool includeStock = true, CancellationToken cancellationToken = default)
        {
            IQueryable<InventoryItem> query = _context.InventoryItems;

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(item => item.Category != null && item.Category == category);
            }

            if (includeStock)
            {
                query = query
                    .Include(item => item.StockLevels)!
                        .ThenInclude(level => level.Warehouse);
            }

            return await query
                .AsNoTracking()
                .OrderBy(item => item.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryItem?> GetByIdAsync(int id, bool includeStock = true, CancellationToken cancellationToken = default)
        {
            IQueryable<InventoryItem> query = _context.InventoryItems.Where(item => item.Id == id);

            if (includeStock)
            {
                query = query
                    .Include(item => item.StockLevels)!
                        .ThenInclude(level => level.Warehouse);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
        {
            await _context.InventoryItems.AddAsync(item, cancellationToken);
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludeItemId = null, CancellationToken cancellationToken = default)
        {
            IQueryable<InventoryItem> query = _context.InventoryItems.AsQueryable();

            if (excludeItemId.HasValue)
            {
                query = query.Where(item => item.Id != excludeItemId.Value);
            }

            return await query.AnyAsync(item => item.Sku == sku, cancellationToken);
        }

        public async Task UpsertStockLevelsAsync(InventoryItem item, IEnumerable<StockLevel> stockLevels, CancellationToken cancellationToken = default)
        {
            var existingLevels = await _context.StockLevels
                .Where(level => level.InventoryItemId == item.Id)
                .ToListAsync(cancellationToken);

            if (existingLevels.Count > 0)
            {
                _context.StockLevels.RemoveRange(existingLevels);
            }

            foreach (var level in stockLevels)
            {
                level.InventoryItemId = item.Id;
            }

            await _context.StockLevels.AddRangeAsync(stockLevels, cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (item == null)
            {
                return false;
            }

            _context.InventoryItems.Remove(item);
            return true;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
