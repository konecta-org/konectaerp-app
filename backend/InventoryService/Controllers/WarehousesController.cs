using AutoMapper;
using InventoryService.Dtos;
using InventoryService.Models;
using InventoryService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<WarehousesController> _logger;

        public WarehousesController(IWarehouseRepository warehouseRepository, IMapper mapper, ILogger<WarehousesController> logger)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseResponseDto>>> GetWarehouses(CancellationToken cancellationToken = default)
        {
            var warehouses = await _warehouseRepository.GetAllAsync(includeStock: true, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<WarehouseResponseDto>>(warehouses));
        }

        [HttpGet("{id:int}", Name = nameof(GetWarehouseById))]
        public async Task<ActionResult<WarehouseResponseDto>> GetWarehouseById(int id, CancellationToken cancellationToken = default)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id, includeStock: true, cancellationToken);
            if (warehouse == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<WarehouseResponseDto>(warehouse));
        }

        [HttpPost]
        public async Task<ActionResult<WarehouseResponseDto>> CreateWarehouse([FromBody] WarehouseUpsertDto request, CancellationToken cancellationToken = default)
        {
            if (await _warehouseRepository.CodeExistsAsync(request.Code, null, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.Code), "Warehouse code already exists.");
                return ValidationProblem(ModelState);
            }

            var warehouse = _mapper.Map<Warehouse>(request);
            await _warehouseRepository.AddAsync(warehouse, cancellationToken);
            await _warehouseRepository.SaveChangesAsync(cancellationToken);

            var created = await _warehouseRepository.GetByIdAsync(warehouse.Id, includeStock: true, cancellationToken);
            var response = _mapper.Map<WarehouseResponseDto>(created);
            return CreatedAtRoute(nameof(GetWarehouseById), new { id = warehouse.Id }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] WarehouseUpsertDto request, CancellationToken cancellationToken = default)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id, includeStock: false, cancellationToken);
            if (warehouse == null)
            {
                return NotFound();
            }

            if (!string.Equals(warehouse.Code, request.Code, StringComparison.OrdinalIgnoreCase) &&
                await _warehouseRepository.CodeExistsAsync(request.Code, id, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.Code), "Warehouse code already exists.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, warehouse);
            await _warehouseRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWarehouse(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var removed = await _warehouseRepository.DeleteAsync(id, cancellationToken);
                if (!removed)
                {
                    return NotFound();
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to delete warehouse {WarehouseId}", id);
                return Conflict(new { message = ex.Message });
            }

            await _warehouseRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
    }
}
