using AutoMapper;
using InventoryService.Dtos;
using InventoryService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IStockTransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public StockTransactionsController(IStockTransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransactionResponseDto>>> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? itemId = null,
            [FromQuery] int? warehouseId = null,
            CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepository.GetRecentAsync(page, pageSize, itemId, warehouseId, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<StockTransactionResponseDto>>(transactions));
        }
    }
}
