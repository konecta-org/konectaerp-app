using AutoMapper;
using HrService.Dtos;
using HrService.Models;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestRepo _leaveRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IMapper _mapper;

        public LeaveRequestsController(
            ILeaveRequestRepo leaveRepo,
            IEmployeeRepo employeeRepo,
            IMapper mapper)
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] Guid? employeeId, [FromQuery] bool pendingOnly = false)
        {
            IEnumerable<LeaveRequest> leaveRequests;

            if (pendingOnly)
            {
                leaveRequests = await _leaveRepo.GetPendingAsync();
            }
            else if (employeeId.HasValue)
            {
                leaveRequests = await _leaveRepo.GetByEmployeeAsync(employeeId.Value);
            }
            else
            {
                leaveRequests = await _leaveRepo.GetAllAsync();
            }

            var response = _mapper.Map<IEnumerable<LeaveRequestResponseDto>>(leaveRequests);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetLeaveRequest(Guid id)
        {
            var leaveRequest = await _leaveRepo.GetByIdAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<LeaveRequestResponseDto>(leaveRequest);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.StartDate > request.EndDate)
            {
                ModelState.AddModelError(nameof(request.StartDate), "Start date cannot be after end date.");
                return ValidationProblem(ModelState);
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound($"Employee {request.EmployeeId} not found.");
            }

            var leaveRequest = _mapper.Map<LeaveRequest>(request);

            await _leaveRepo.AddAsync(leaveRequest);
            await _leaveRepo.SaveChangesAsync();

            leaveRequest = await _leaveRepo.GetByIdAsync(leaveRequest.Id);
            var response = _mapper.Map<LeaveRequestResponseDto>(leaveRequest);
            return CreatedAtAction(nameof(GetLeaveRequest), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateLeaveRequest(Guid id, [FromBody] UpdateLeaveRequestDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.StartDate > request.EndDate)
            {
                ModelState.AddModelError(nameof(request.StartDate), "Start date cannot be after end date.");
                return ValidationProblem(ModelState);
            }

            var leaveRequest = await _leaveRepo.GetByIdAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            if (request.ApprovedByEmployeeId.HasValue)
            {
                var approver = await _employeeRepo.GetEmployeeByIdAsync(request.ApprovedByEmployeeId.Value);
                if (approver == null)
                {
                    return NotFound($"Approver {request.ApprovedByEmployeeId} not found.");
                }
            }

            _mapper.Map(request, leaveRequest);

            await _leaveRepo.UpdateAsync(leaveRequest);
            await _leaveRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLeaveRequest(Guid id)
        {
            var deleted = await _leaveRepo.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _leaveRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
