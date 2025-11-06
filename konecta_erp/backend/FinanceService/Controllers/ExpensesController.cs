using AutoMapper;
using FinanceService.Dtos;
using FinanceService.Models;
using FinanceService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.Authorization;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(IExpenseRepository expenseRepository, IMapper mapper, ILogger<ExpensesController> logger)
        {
            _expenseRepository = expenseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = PermissionConstants.Finance.ExpensesRead)]
        public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetExpenses([FromQuery] string? category = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken cancellationToken = default)
        {
            var expenses = await _expenseRepository.GetAllAsync(category, from, to, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses));
        }

        [HttpGet("{id:int}", Name = nameof(GetExpenseById))]
        [Authorize(Policy = PermissionConstants.Finance.ExpensesRead)]
        public async Task<ActionResult<ExpenseResponseDto>> GetExpenseById(int id, CancellationToken cancellationToken = default)
        {
            var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
            if (expense == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ExpenseResponseDto>(expense));
        }

        [HttpPost]
        [Authorize(Policy = PermissionConstants.Finance.ExpensesManage)]
        public async Task<ActionResult<ExpenseResponseDto>> CreateExpense([FromBody] ExpenseUpsertDto request, CancellationToken cancellationToken = default)
        {
            var expense = _mapper.Map<Expense>(request);
            expense.Amount = RoundCurrency(expense.Amount);

            await _expenseRepository.AddAsync(expense, cancellationToken);
            await _expenseRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<ExpenseResponseDto>(expense);
            return CreatedAtRoute(nameof(GetExpenseById), new { id = expense.Id }, response);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = PermissionConstants.Finance.ExpensesManage)]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseUpsertDto request, CancellationToken cancellationToken = default)
        {
            var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
            if (expense == null)
            {
                return NotFound();
            }

            _mapper.Map(request, expense);
            expense.Amount = RoundCurrency(expense.Amount);

            await _expenseRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = PermissionConstants.Finance.ExpensesManage)]
        public async Task<IActionResult> DeleteExpense(int id, CancellationToken cancellationToken = default)
        {
            var removed = await _expenseRepository.DeleteAsync(id, cancellationToken);
            if (!removed)
            {
                return NotFound();
            }

            await _expenseRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
