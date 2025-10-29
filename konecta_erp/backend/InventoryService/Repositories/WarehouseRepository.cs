using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync(bool includeStock = true, CancellationToken cancellationToken = default)
        {
            IQueryable<Warehouse> query = _context.Warehouses;

            if (includeStock)
            {
                query = query
                    .Include(warehouse => warehouse.StockLevels)!
                        .ThenInclude(level => level.InventoryItem);
            }

            return await query
                .AsNoTracking()
                .OrderBy(warehouse => warehouse.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Warehouse?> GetByIdAsync(int id, bool includeStock = true, CancellationToken cancellationToken = default)
        {
            IQueryable<Warehouse> query = _context.Warehouses.Where(warehouse => warehouse.Id == id);

            if (includeStock)
            {
                query = query
                    .Include(warehouse => warehouse.StockLevels)!
                        .ThenInclude(level => level.InventoryItem);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
        {
            await _context.Warehouses.AddAsync(warehouse, cancellationToken);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Warehouse> query = _context.Warehouses.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(warehouse => warehouse.Id != excludeId.Value);
            }

            return await query.AnyAsync(warehouse => warehouse.Code == code, cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.StockLevels)
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            if (warehouse == null)
            {
                return false;
            }

            if (warehouse.StockLevels != null && warehouse.StockLevels.Any(level => level.QuantityOnHand > 0))
            {
                throw new InvalidOperationException("Warehouse cannot be deleted while stock is available.");
            }

            _context.Warehouses.Remove(warehouse);
            return true;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
