using AutoMapper;
using HrService.Dtos;
using HrService.Models;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepo _departmentRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(
            IDepartmentRepo departmentRepo,
            IEmployeeRepo employeeRepo,
            IMapper mapper,
            ILogger<DepartmentController> logger)
        {
            _departmentRepo = departmentRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
        {
            var departments = await _departmentRepo.GetAllDepartmentsAsync(includeEmployees: true);
            var response = _mapper.Map<IEnumerable<DepartmentResponseDto>>(departments);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDepartment(Guid id, CancellationToken cancellationToken)
        {
            var department = await _departmentRepo.GetDepartmentByIdAsync(id, includeEmployees: true);
            if (department == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<DepartmentResponseDto>(department);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (await _departmentRepo.DepartmentNameExistsAsync(request.DepartmentName))
            {
                ModelState.AddModelError(nameof(request.DepartmentName), "Department name already exists.");
                return ValidationProblem(ModelState);
            }

            var normalizedManagerId = request.ManagerId.HasValue && request.ManagerId.Value != Guid.Empty
                ? request.ManagerId
                : null;

            if (normalizedManagerId.HasValue)
            {
                var manager = await _employeeRepo.GetEmployeeByIdAsync(normalizedManagerId.Value);
                if (manager == null)
                {
                    return NotFound($"Manager {normalizedManagerId} not found.");
                }
            }

            var department = _mapper.Map<Department>(request);
            department.ManagerId = normalizedManagerId;
            await _departmentRepo.AddDepartmentAsync(department);
            await _departmentRepo.SaveChangesAsync();

            var response = _mapper.Map<DepartmentResponseDto>(department);
            return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto request, CancellationToken cancellationToken)
        {
            if (id != request.DepartmentId)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (await _departmentRepo.DepartmentNameExistsAsync(request.DepartmentName, request.DepartmentId))
            {
                ModelState.AddModelError(nameof(request.DepartmentName), "Department name already exists.");
                return ValidationProblem(ModelState);
            }

            var normalizedManagerId = request.ManagerId.HasValue && request.ManagerId.Value != Guid.Empty
                ? request.ManagerId
                : null;

            if (normalizedManagerId.HasValue)
            {
                var manager = await _employeeRepo.GetEmployeeByIdAsync(normalizedManagerId.Value);
                if (manager == null)
                {
                    return NotFound($"Manager {normalizedManagerId} not found.");
                }
            }

            var department = await _departmentRepo.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            _mapper.Map(request, department);
            department.ManagerId = normalizedManagerId;
            await _departmentRepo.UpdateDepartmentAsync(department);
            await _departmentRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _departmentRepo.DeleteDepartmentAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _departmentRepo.SaveChangesAsync();
            _logger.LogInformation("Department {DepartmentId} deleted.", id);
            return NoContent();
        }

        [HttpPut("{id:guid}/manager")]
        public async Task<IActionResult> AssignManager(Guid id, [FromBody] AssignDepartmentManagerDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var department = await _departmentRepo.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound($"Department {id} not found.");
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound($"Employee {request.EmployeeId} not found.");
            }

            if (employee.DepartmentId != id)
            {
                return BadRequest("Employee must belong to the department before being assigned as manager.");
            }

            var updated = await _departmentRepo.AssignManagerAsync(id, request.EmployeeId);
            if (!updated)
            {
                return NotFound($"Department {id} not found.");
            }

            await _departmentRepo.SaveChangesAsync();
            _logger.LogInformation("Assigned employee {EmployeeId} as manager for department {DepartmentId}.", request.EmployeeId, id);
            return NoContent();
        }
    }
}
