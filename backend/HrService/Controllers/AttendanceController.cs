using AutoMapper;
using HrService.Dtos;
using HrService.Models;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepo _attendanceRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IMapper _mapper;

        public AttendanceController(
            IAttendanceRepo attendanceRepo,
            IEmployeeRepo employeeRepo,
            IMapper mapper)
        {
            _attendanceRepo = attendanceRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceRecords([FromQuery] Guid? employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            IEnumerable<AttendanceRecord> records;

            if (employeeId.HasValue)
            {
                records = await _attendanceRepo.GetByEmployeeAsync(employeeId.Value, startDate, endDate);
            }
            else
            {
                records = await _attendanceRepo.GetAllAsync();
            }

            var response = _mapper.Map<IEnumerable<AttendanceRecordResponseDto>>(records);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAttendanceRecord(Guid id)
        {
            var record = await _attendanceRepo.GetByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<AttendanceRecordResponseDto>(record);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttendanceRecord([FromBody] CreateAttendanceRecordDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.CheckInTime.HasValue && request.CheckOutTime.HasValue && request.CheckOutTime < request.CheckInTime)
            {
                ModelState.AddModelError(nameof(request.CheckOutTime), "Check-out time cannot be before check-in time.");
                return ValidationProblem(ModelState);
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound($"Employee {request.EmployeeId} not found.");
            }

            var existing = await _attendanceRepo.GetByEmployeeAndDateAsync(request.EmployeeId, request.WorkDate);
            if (existing != null)
            {
                return Conflict("An attendance record already exists for this employee and date.");
            }

            var record = _mapper.Map<AttendanceRecord>(request);
            await _attendanceRepo.AddAsync(record);
            await _attendanceRepo.SaveChangesAsync();

            record = await _attendanceRepo.GetByIdAsync(record.Id);
            var response = _mapper.Map<AttendanceRecordResponseDto>(record);
            return CreatedAtAction(nameof(GetAttendanceRecord), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAttendanceRecord(Guid id, [FromBody] UpdateAttendanceRecordDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.CheckInTime.HasValue && request.CheckOutTime.HasValue && request.CheckOutTime < request.CheckInTime)
            {
                ModelState.AddModelError(nameof(request.CheckOutTime), "Check-out time cannot be before check-in time.");
                return ValidationProblem(ModelState);
            }

            var record = await _attendanceRepo.GetByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            _mapper.Map(request, record);

            await _attendanceRepo.UpdateAsync(record);
            await _attendanceRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAttendanceRecord(Guid id)
        {
            var deleted = await _attendanceRepo.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _attendanceRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
