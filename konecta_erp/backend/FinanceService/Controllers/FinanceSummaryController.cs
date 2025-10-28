using FinanceService.Dtos;
using FinanceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/finance-summary")]
    public class FinanceSummaryController : ControllerBase
    {
        private readonly IFinanceSummaryService _summaryService;
        private readonly ILogger<FinanceSummaryController> _logger;

        public FinanceSummaryController(IFinanceSummaryService summaryService, ILogger<FinanceSummaryController> logger)
        {
            _summaryService = summaryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<FinanceSummaryDto>> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _summaryService.BuildSummaryAsync(cancellationToken);
            return Ok(summary);
        }
    }
}
