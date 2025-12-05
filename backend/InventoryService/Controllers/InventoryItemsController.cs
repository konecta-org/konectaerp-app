using AutoMapper;
using InventoryService.Dtos;
using InventoryService.Models;
using InventoryService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryItemsController : ControllerBase
    {
        private readonly IInventoryItemRepository _itemRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryItemsController> _logger;

        public InventoryItemsController(
            IInventoryItemRepository itemRepository,
            IWarehouseRepository warehouseRepository,
            IMapper mapper,
            ILogger<InventoryItemsController> logger)
        {
            _itemRepository = itemRepository;
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemResponseDto>>> GetItems([FromQuery] string? category = null, CancellationToken cancellationToken = default)
        {
            var items = await _itemRepository.GetAllAsync(category, includeStock: true, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<InventoryItemResponseDto>>(items));
        }

        [HttpGet("{id:int}", Name = nameof(GetItemById))]
        public async Task<ActionResult<InventoryItemResponseDto>> GetItemById(int id, CancellationToken cancellationToken = default)
        {
            var item = await _itemRepository.GetByIdAsync(id, includeStock: true, cancellationToken);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<InventoryItemResponseDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItemResponseDto>> CreateItem([FromBody] InventoryItemUpsertDto request, CancellationToken cancellationToken = default)
        {
            if (await _itemRepository.SkuExistsAsync(request.Sku, null, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.Sku), "SKU already exists.");
                return ValidationProblem(ModelState);
            }

            if (!await ValidateStockLevelsAsync(request.StockLevels, cancellationToken))
            {
                return ValidationProblem(ModelState);
            }

            var item = _mapper.Map<InventoryItem>(request);
            await _itemRepository.AddAsync(item, cancellationToken);
            await _itemRepository.SaveChangesAsync(cancellationToken);

            if (request.StockLevels.Count > 0)
            {
                var stockLevels = request.StockLevels.Select(sl => CreateStockLevel(item.Id, sl)).ToList();
                await _itemRepository.UpsertStockLevelsAsync(item, stockLevels, cancellationToken);
                await _itemRepository.SaveChangesAsync(cancellationToken);
            }

            var created = await _itemRepository.GetByIdAsync(item.Id, includeStock: true, cancellationToken);
            var response = _mapper.Map<InventoryItemResponseDto>(created);
            return CreatedAtRoute(nameof(GetItemById), new { id = item.Id }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] InventoryItemUpsertDto request, CancellationToken cancellationToken = default)
        {
            var existing = await _itemRepository.GetByIdAsync(id, includeStock: true, cancellationToken);
            if (existing == null)
            {
                return NotFound();
            }

            if (!string.Equals(existing.Sku, request.Sku, StringComparison.OrdinalIgnoreCase) &&
                await _itemRepository.SkuExistsAsync(request.Sku, id, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.Sku), "SKU already exists.");
                return ValidationProblem(ModelState);
            }

            if (!await ValidateStockLevelsAsync(request.StockLevels, cancellationToken))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, existing);
            await _itemRepository.SaveChangesAsync(cancellationToken);

            var stockLevels = request.StockLevels.Select(sl => CreateStockLevel(existing.Id, sl)).ToList();
            await _itemRepository.UpsertStockLevelsAsync(existing, stockLevels, cancellationToken);
            await _itemRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteItem(int id, CancellationToken cancellationToken = default)
        {
            var removed = await _itemRepository.DeleteAsync(id, cancellationToken);
            if (!removed)
            {
                return NotFound();
            }

            await _itemRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private async Task<bool> ValidateStockLevelsAsync(IEnumerable<StockLevelUpsertDto> stockLevels, CancellationToken cancellationToken)
        {
            if (stockLevels == null)
            {
                return true;
            }

            var duplicates = stockLevels.GroupBy(sl => sl.WarehouseId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                ModelState.AddModelError(nameof(InventoryItemUpsertDto.StockLevels), $"Duplicate warehouse references found: {string.Join(", ", duplicates)}");
                return false;
            }

            foreach (var stock in stockLevels)
            {
                if (stock.QuantityReserved > stock.QuantityOnHand)
                {
                    ModelState.AddModelError(nameof(stock.QuantityReserved), "Reserved quantity cannot exceed on-hand quantity.");
                }

                var warehouseExists = await _warehouseRepository.GetByIdAsync(stock.WarehouseId, includeStock: false, cancellationToken);
                if (warehouseExists == null)
                {
                    ModelState.AddModelError(nameof(stock.WarehouseId), $"Warehouse {stock.WarehouseId} not found.");
                }
            }

            return ModelState.ErrorCount == 0;
        }

        private static StockLevel CreateStockLevel(int itemId, StockLevelUpsertDto dto)
        {
            return new StockLevel
            {
                InventoryItemId = itemId,
                WarehouseId = dto.WarehouseId,
                QuantityOnHand = Round(dto.QuantityOnHand),
                QuantityReserved = Round(Math.Min(dto.QuantityReserved, dto.QuantityOnHand)),
                ReorderQuantity = Round(dto.ReorderQuantity)
            };
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
