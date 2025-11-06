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
    public class PayrollRunsController : ControllerBase
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PayrollRunsController> _logger;

        public PayrollRunsController(IPayrollRepository payrollRepository, IMapper mapper, ILogger<PayrollRunsController> logger)
        {
            _payrollRepository = payrollRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = PermissionConstants.Finance.PayrollRead)]
        public async Task<ActionResult<IEnumerable<PayrollRunResponseDto>>> GetPayrollRuns([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] bool includeEntries = false, CancellationToken cancellationToken = default)
        {
            var runs = await _payrollRepository.GetAllAsync(from, to, includeEntries, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<PayrollRunResponseDto>>(runs));
        }

        [HttpGet("{id:int}", Name = nameof(GetPayrollRunById))]
        [Authorize(Policy = PermissionConstants.Finance.PayrollRead)]
        public async Task<ActionResult<PayrollRunResponseDto>> GetPayrollRunById(int id, [FromQuery] bool includeEntries = true, CancellationToken cancellationToken = default)
        {
            var run = await _payrollRepository.GetByIdAsync(id, includeEntries, cancellationToken);
            if (run == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PayrollRunResponseDto>(run));
        }

        [HttpPost]
        [Authorize(Policy = PermissionConstants.Finance.PayrollManage)]
        public async Task<ActionResult<PayrollRunResponseDto>> CreatePayrollRun([FromBody] PayrollRunUpsertDto request, CancellationToken cancellationToken = default)
        {
            if (request.PeriodStart > request.PeriodEnd)
            {
                ModelState.AddModelError(nameof(request.PeriodStart), "Period start must be earlier than period end.");
                return ValidationProblem(ModelState);
            }

            if (await _payrollRepository.PayrollNumberExistsAsync(request.PayrollNumber, null, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.PayrollNumber), "Payroll number already exists.");
                return ValidationProblem(ModelState);
            }

            var payrollRun = _mapper.Map<PayrollRun>(request);
            var entries = request.Entries.Select(dto => _mapper.Map<PayrollEntry>(dto)).ToList();
            ApplyPayrollCalculations(payrollRun, request, entries);
            payrollRun.Entries = entries;

            await _payrollRepository.AddAsync(payrollRun, cancellationToken);
            await _payrollRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<PayrollRunResponseDto>(payrollRun);
            return CreatedAtRoute(nameof(GetPayrollRunById), new { id = payrollRun.Id }, response);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = PermissionConstants.Finance.PayrollManage)]
        public async Task<IActionResult> UpdatePayrollRun(int id, [FromBody] PayrollRunUpsertDto request, CancellationToken cancellationToken = default)
        {
            if (request.PeriodStart > request.PeriodEnd)
            {
                ModelState.AddModelError(nameof(request.PeriodStart), "Period start must be earlier than period end.");
                return ValidationProblem(ModelState);
            }

            var run = await _payrollRepository.GetByIdAsync(id, includeEntries: true, cancellationToken);
            if (run == null)
            {
                return NotFound();
            }

            if (!string.Equals(run.PayrollNumber, request.PayrollNumber, StringComparison.OrdinalIgnoreCase) &&
                await _payrollRepository.PayrollNumberExistsAsync(request.PayrollNumber, id, cancellationToken))
            {
                ModelState.AddModelError(nameof(request.PayrollNumber), "Payroll number already exists.");
                return ValidationProblem(ModelState);
            }

            _mapper.Map(request, run);
            var entries = request.Entries.Select(dto => _mapper.Map<PayrollEntry>(dto)).ToList();
            ApplyPayrollCalculations(run, request, entries);

            await _payrollRepository.UpdateEntriesAsync(run, entries, cancellationToken);
            run.Entries = entries;

            await _payrollRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = PermissionConstants.Finance.PayrollManage)]
        public async Task<IActionResult> DeletePayrollRun(int id, CancellationToken cancellationToken = default)
        {
            var removed = await _payrollRepository.DeleteAsync(id, cancellationToken);
            if (!removed)
            {
                return NotFound();
            }

            await _payrollRepository.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private static void ApplyPayrollCalculations(PayrollRun payrollRun, PayrollRunUpsertDto request, List<PayrollEntry> entries)
        {
            foreach (var entry in entries)
            {
                entry.GrossPay = RoundCurrency(entry.GrossPay);
                entry.NetPay = RoundCurrency(Math.Clamp(entry.NetPay, 0, entry.GrossPay));
                entry.Deductions = RoundCurrency(entry.Deductions);
                entry.Taxes = RoundCurrency(entry.Taxes);
            }

            var grossTotal = RoundCurrency(entries.Sum(entry => entry.GrossPay));
            var netTotal = RoundCurrency(entries.Sum(entry => entry.NetPay));

            payrollRun.TotalGrossPay = RoundCurrency(Math.Max(request.TotalGrossPay, grossTotal));
            payrollRun.TotalNetPay = RoundCurrency(Math.Clamp(Math.Max(request.TotalNetPay, netTotal), 0, payrollRun.TotalGrossPay));
        }

        private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
