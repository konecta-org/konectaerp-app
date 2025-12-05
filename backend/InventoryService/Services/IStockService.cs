using InventoryService.Dtos;
using InventoryService.Models;

namespace InventoryService.Services
{
    public interface IStockService
    {
        Task<StockTransaction> AdjustStockAsync(StockAdjustmentRequestDto request, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockTransaction>> TransferStockAsync(StockTransferRequestDto request, CancellationToken cancellationToken = default);
    }
}
