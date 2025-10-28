using FinanceService.Dtos;

namespace FinanceService.Services
{
    public interface IFinanceSummaryService
    {
        Task<FinanceSummaryDto> BuildSummaryAsync(CancellationToken cancellationToken = default);
    }
}
