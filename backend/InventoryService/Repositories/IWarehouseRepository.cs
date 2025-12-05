using InventoryService.Models;

namespace InventoryService.Repositories
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<Warehouse>> GetAllAsync(bool includeStock = true, CancellationToken cancellationToken = default);
        Task<Warehouse?> GetByIdAsync(int id, bool includeStock = true, CancellationToken cancellationToken = default);
        Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
