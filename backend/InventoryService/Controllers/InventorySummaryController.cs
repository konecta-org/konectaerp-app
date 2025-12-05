using InventoryService.Dtos;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/inventory-summary")]
    public class InventorySummaryController : ControllerBase
    {
        private readonly IInventorySummaryService _summaryService;

        public InventorySummaryController(IInventorySummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        [HttpGet]
        public async Task<ActionResult<InventorySummaryDto>> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _summaryService.BuildSummaryAsync(cancellationToken);
            return Ok(summary);
        }
    }
}
