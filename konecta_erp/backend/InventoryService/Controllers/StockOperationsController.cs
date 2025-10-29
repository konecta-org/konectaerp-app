using AutoMapper;
using InventoryService.Dtos;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/stock-operations")]
    public class StockOperationsController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;
        private readonly ILogger<StockOperationsController> _logger;

        public StockOperationsController(IStockService stockService, IMapper mapper, ILogger<StockOperationsController> logger)
        {
            _stockService = stockService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("adjust")]
        public async Task<ActionResult<StockTransactionResponseDto>> AdjustStock([FromBody] StockAdjustmentRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var transaction = await _stockService.AdjustStockAsync(request, cancellationToken);
                return Ok(_mapper.Map<StockTransactionResponseDto>(transaction));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to adjust stock for item {Item} in warehouse {Warehouse}", request.InventoryItemId, request.WarehouseId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("transfer")]
        public async Task<ActionResult<IEnumerable<StockTransactionResponseDto>>> TransferStock([FromBody] StockTransferRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _stockService.TransferStockAsync(request, cancellationToken);
                var response = _mapper.Map<IEnumerable<StockTransactionResponseDto>>(transactions);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to transfer stock for item {Item} from {FromWarehouse} to {ToWarehouse}", request.InventoryItemId, request.FromWarehouseId, request.ToWarehouseId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("transaction-types")]
        public ActionResult<IEnumerable<string>> GetTransactionTypes()
        {
            return Ok(StockTransactionTypes.All);
        }
    }
}
