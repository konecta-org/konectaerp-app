using AutoMapper;
using FinanceService.Dtos;
using FinanceService.Models;
using FinanceService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BudgetsController> _logger;

        public BudgetsController(IBudgetRepository budgetRepository, IMapper mapper, ILogger<BudgetsController> logger)
        {
            _budgetRepository = budgetRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetBudgets([FromQuery] int? fiscalYear = null, [FromQuery] bool includeLines = true, CancellationToken cancellationToken = default)
        {
            var budgets = await _budgetRepository.GetAllAsync(fiscalYear, includeLines, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<BudgetResponseDto>>(budgets));
        }

        [HttpGet("{id:int}", Name = nameof(GetBudgetById))]
        public async Task<ActionResult<BudgetResponseDto>> GetBudgetById(int id, [FromQuery] bool includeLines = true, CancellationToken cancellationToken = default)
        {
            var budget = await _budgetRepository.GetByIdAsync(id, includeLines, cancellationToken);
            if (budget == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BudgetResponseDto>(budget));
        }

        [HttpPost]
        public async Task<ActionResult<BudgetResponseDto>> CreateBudget([FromBody] BudgetUpsertDto request, CancellationToken cancellationToken = default)
        {
            var budget = _mapper.Map<Budget>(request);
            var lines = request.Lines.Select(lineDto => _mapper.Map<BudgetLine>(lineDto)).ToList();
            ApplyBudgetCalculations(budget, request, lines);

            budget.Lines = lines;

            await _budgetRepository.AddAsync(budget, cancellationToken);
            await _budgetRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<BudgetResponseDto>(budget);
            return CreatedAtRoute(nameof(GetBudgetById), new { id = budget.Id }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] BudgetUpsertDto request, CancellationToken cancellationToken = default)
        {
            var budget = await _budgetRepository.GetByIdAsync(id, includeLines: true, cancellationToken);
            if (budget == null)
            {
                return NotFound();
            }

            _mapper.Map(request, budget);
            var lines = request.Lines.Select(lineDto => _mapper.Map<BudgetLine>(lineDto)).ToList();
            ApplyBudgetCalculations(budget, request, lines);

            await _budgetRepository.UpdateLinesAsync(budget, lines, cancellationToken);
            budget.Lines = lines;

            await _budgetRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBudget(int id, CancellationToken cancellationToken = default)
        {
            var removed = await _budgetRepository.DeleteAsync(id, cancellationToken);
            if (!removed)
            {
                return NotFound();
            }

            await _budgetRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private static void ApplyBudgetCalculations(Budget budget, BudgetUpsertDto request, List<BudgetLine> lines)
        {
            foreach (var line in lines)
            {
                line.AllocatedAmount = RoundCurrency(line.AllocatedAmount);
                line.SpentAmount = RoundCurrency(Math.Clamp(line.SpentAmount, 0, line.AllocatedAmount));
            }

            var totalAllocated = RoundCurrency(lines.Sum(line => line.AllocatedAmount));
            var totalSpent = RoundCurrency(lines.Sum(line => line.SpentAmount));

            budget.TotalAmount = RoundCurrency(Math.Max(request.TotalAmount, totalAllocated));
            budget.SpentAmount = RoundCurrency(Math.Clamp(Math.Max(request.SpentAmount, totalSpent), 0, budget.TotalAmount));
        }

        private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
