using AutoMapper;
using HrService.Dtos;
using HrService.Messaging;
using HrService.Models;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedContracts.Events;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/employee")]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IDepartmentRepo _departmentRepo;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly RabbitMqOptions _rabbitOptions;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeRepo employeeRepo,
            IDepartmentRepo departmentRepo,
            IMapper mapper,
            IEventPublisher eventPublisher,
            IOptions<RabbitMqOptions> rabbitOptions,
            ILogger<EmployeeController> logger)
        {
            _employeeRepo = employeeRepo;
            _departmentRepo = departmentRepo;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _rabbitOptions = rabbitOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees(CancellationToken cancellationToken)
        {
            var employees = await _employeeRepo.GetAllEmployeesAsync();
            var response = _mapper.Map<IEnumerable<EmployeeResponseDto>>(employees);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEmployeeById(Guid id, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(id, includeDepartment: true);
            if (employee == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<EmployeeResponseDto>(employee);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (await _employeeRepo.WorkEmailExistsAsync(request.WorkEmail))
            {
                ModelState.AddModelError(nameof(request.WorkEmail), "Work email already in use.");
                return ValidationProblem(ModelState);
            }

            var department = await _departmentRepo.GetDepartmentByIdAsync(request.DepartmentId);
            if (department == null)
            {
                return NotFound($"Department {request.DepartmentId} not found.");
            }

            var employee = _mapper.Map<Employee>(request);
            employee.Department = department;
            employee.PhoneNumber = request.PhoneNumber?.Trim();
            employee.Salary = Math.Round(request.Salary, 2, MidpointRounding.AwayFromZero);

            await _employeeRepo.AddEmployeeAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            var response = _mapper.Map<EmployeeResponseDto>(employee);

            var employeeCreatedEvent = new EmployeeCreatedEvent(
                employee.Id,
                employee.FullName,
                employee.WorkEmail,
                employee.PersonalEmail,
                employee.Position,
                department.DepartmentId,
                department.DepartmentName,
                employee.HireDate);

            await _eventPublisher.PublishAsync(_rabbitOptions.EmployeeCreatedRoutingKey, employeeCreatedEvent, cancellationToken);

            _logger.LogInformation("Employee {EmployeeId} created and event published.", employee.Id);

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto request, CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (await _employeeRepo.WorkEmailExistsAsync(request.WorkEmail, request.Id))
            {
                ModelState.AddModelError(nameof(request.WorkEmail), "Work email already in use.");
                return ValidationProblem(ModelState);
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var department = await _departmentRepo.GetDepartmentByIdAsync(request.DepartmentId);
            if (department == null)
            {
                return NotFound($"Department {request.DepartmentId} not found.");
            }

            _mapper.Map(request, employee);
            employee.Department = department;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.PhoneNumber = request.PhoneNumber?.Trim();
            employee.Salary = Math.Round(request.Salary, 2, MidpointRounding.AwayFromZero);

            await _employeeRepo.UpdateEmployeeAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:guid}/bonuses")]
        public async Task<IActionResult> IssueBonuses(Guid id, [FromBody] IssueEmployeeBonusesDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.Bonuses.Count == 0)
            {
                ModelState.AddModelError(nameof(request.Bonuses), "At least one bonus entry is required.");
                return ValidationProblem(ModelState);
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(id, includeDepartment: true);
            if (employee == null)
            {
                return NotFound();
            }

            var bonusItems = request.Bonuses.Select(b => new EmployeeCompensationBonusItem(
                b.BonusType,
                Math.Round(b.Amount, 2, MidpointRounding.AwayFromZero),
                b.AwardedOn,
                b.Period,
                b.Reference,
                string.IsNullOrWhiteSpace(b.AwardedBy) ? request.IssuedBy : b.AwardedBy,
                b.Notes,
                b.SourceSystem)).ToList();

            var bonusEvent = new EmployeeCompensationBonusesIssuedEvent(
                employee.Id,
                employee.FullName,
                bonusItems,
                DateTime.UtcNow,
                request.IssuedBy);

            await _eventPublisher.PublishAsync(_rabbitOptions.FinanceCompensationBonusesRoutingKey, bonusEvent, cancellationToken);
            _logger.LogInformation("Published {BonusCount} bonuses for employee {EmployeeId}.", bonusItems.Count, employee.Id);

            return Accepted();
        }

        [HttpPost("{id:guid}/deductions")]
        public async Task<IActionResult> IssueDeductions(Guid id, [FromBody] IssueEmployeeDeductionsDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.Deductions.Count == 0)
            {
                ModelState.AddModelError(nameof(request.Deductions), "At least one deduction entry is required.");
                return ValidationProblem(ModelState);
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(id, includeDepartment: true);
            if (employee == null)
            {
                return NotFound();
            }

            var deductionItems = request.Deductions.Select(d => new EmployeeCompensationDeductionItem(
                d.DeductionType,
                Math.Round(d.Amount, 2, MidpointRounding.AwayFromZero),
                d.AppliedOn,
                d.Period,
                d.Reference,
                string.IsNullOrWhiteSpace(d.AppliedBy) ? request.IssuedBy : d.AppliedBy,
                d.Notes,
                d.SourceSystem,
                d.IsRecurring)).ToList();

            var deductionEvent = new EmployeeCompensationDeductionsIssuedEvent(
                employee.Id,
                employee.FullName,
                deductionItems,
                DateTime.UtcNow,
                request.IssuedBy);

            await _eventPublisher.PublishAsync(_rabbitOptions.FinanceCompensationDeductionsRoutingKey, deductionEvent, cancellationToken);
            _logger.LogInformation("Published {DeductionCount} deductions for employee {EmployeeId}.", deductionItems.Count, employee.Id);

            return Accepted();
        }

        [HttpPost("{id:guid}/fire")]
        public async Task<IActionResult> FireEmployee(Guid id, [FromBody] FireEmployeeRequestDto? request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var reason = string.IsNullOrWhiteSpace(request?.Reason) ? "Terminated" : request!.Reason!.Trim();
            var eligibleForRehire = request?.EligibleForRehire;

            var terminated = await _employeeRepo.TerminateEmployeeAsync(id, reason, eligibleForRehire);
            if (!terminated)
            {
                return NotFound();
            }

            await _employeeRepo.SaveChangesAsync();

            var terminationEvent = new EmployeeTerminatedEvent(
                employee.Id,
                employee.UserId,
                DateTime.UtcNow,
                reason,
                eligibleForRehire ?? false);

            await _eventPublisher.PublishAsync(_rabbitOptions.EmployeeTerminatedRoutingKey, terminationEvent, cancellationToken);
            _logger.LogInformation("Employee {EmployeeId} terminated and event published.", employee.Id);

            return NoContent();
        }
    }
}

