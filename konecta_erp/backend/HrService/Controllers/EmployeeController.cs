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
    [Route("api/[controller]")]
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

            await _employeeRepo.UpdateEmployeeAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _employeeRepo.DeleteEmployeeAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _employeeRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
