using InventoryService.Dtos;

namespace InventoryService.Services
{
    public interface IInventorySummaryService
    {
        Task<InventorySummaryDto> BuildSummaryAsync(CancellationToken cancellationToken = default);
    }
}
