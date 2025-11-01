using FinanceService.Dtos;
using FinanceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeCompensationController : ControllerBase
    {
        private readonly IEmployeeCompensationService _compensationService;
        private readonly ILogger<EmployeeCompensationController> _logger;

        public EmployeeCompensationController(
            IEmployeeCompensationService compensationService,
            ILogger<EmployeeCompensationController> logger)
        {
            _compensationService = compensationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeCompensationResponseDto>> CreateOrUpdateAccount(
            [FromBody] EmployeeAccountUpsertDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var response = await _compensationService.UpsertAccountAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{employeeId}")]
        public async Task<ActionResult<EmployeeCompensationResponseDto>> GetAccountSummary(
            string employeeId,
            CancellationToken cancellationToken)
        {
            var summary = await _compensationService.GetAccountSummaryAsync(employeeId, cancellationToken);
            if (summary == null)
            {
                return NotFound();
            }

            return Ok(summary);
        }

        [HttpPost("{employeeId}/bonuses")]
        public async Task<ActionResult<IEnumerable<EmployeeBonusResponseDto>>> AddBonuses(
            string employeeId,
            [FromBody] IEnumerable<CompensationBonusCreateDto> bonuses,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var result = await _compensationService.AddBonusesAsync(employeeId, bonuses, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to add bonuses for employee {EmployeeId}", employeeId);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failure while adding bonuses for employee {EmployeeId}", employeeId);
                ModelState.AddModelError(nameof(bonuses), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPost("{employeeId}/deductions")]
        public async Task<ActionResult<IEnumerable<EmployeeDeductionResponseDto>>> AddDeductions(
            string employeeId,
            [FromBody] IEnumerable<CompensationDeductionCreateDto> deductions,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var result = await _compensationService.AddDeductionsAsync(employeeId, deductions, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Unable to add deductions for employee {EmployeeId}", employeeId);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failure while adding deductions for employee {EmployeeId}", employeeId);
                ModelState.AddModelError(nameof(deductions), ex.Message);
                return ValidationProblem(ModelState);
            }
        }
    }
}
