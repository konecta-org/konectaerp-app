using AutoMapper;
using HrService.Dtos;
using HrService.Models;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobOpeningsController : ControllerBase
    {
        private readonly IJobOpeningRepo _jobOpeningRepo;
        private readonly IDepartmentRepo _departmentRepo;
        private readonly IMapper _mapper;

        public JobOpeningsController(
            IJobOpeningRepo jobOpeningRepo,
            IDepartmentRepo departmentRepo,
            IMapper mapper)
        {
            _jobOpeningRepo = jobOpeningRepo;
            _departmentRepo = departmentRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobOpenings([FromQuery] Guid? departmentId, [FromQuery] bool includeApplications = false)
        {
            IEnumerable<JobOpening> jobOpenings;
            if (departmentId.HasValue)
            {
                jobOpenings = await _jobOpeningRepo.GetByDepartmentAsync(departmentId.Value);
            }
            else
            {
                jobOpenings = await _jobOpeningRepo.GetAllAsync(includeApplications);
            }

            var response = _mapper.Map<IEnumerable<JobOpeningResponseDto>>(jobOpenings);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetJobOpening(Guid id, [FromQuery] bool includeApplications = true)
        {
            var jobOpening = await _jobOpeningRepo.GetByIdAsync(id, includeApplications);
            if (jobOpening == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<JobOpeningResponseDto>(jobOpening);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobOpening([FromBody] CreateJobOpeningDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.SalaryMin.HasValue && request.SalaryMax.HasValue && request.SalaryMin > request.SalaryMax)
            {
                ModelState.AddModelError(nameof(request.SalaryMin), "Minimum salary cannot exceed maximum salary.");
                return ValidationProblem(ModelState);
            }

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepo.GetDepartmentByIdAsync(request.DepartmentId.Value);
                if (department == null)
                {
                    return NotFound($"Department {request.DepartmentId} not found.");
                }
            }

            var jobOpening = _mapper.Map<JobOpening>(request);
            jobOpening.PostedDate = DateTime.UtcNow;

            await _jobOpeningRepo.AddAsync(jobOpening);
            await _jobOpeningRepo.SaveChangesAsync();

            var response = _mapper.Map<JobOpeningResponseDto>(jobOpening);
            return CreatedAtAction(nameof(GetJobOpening), new { id = jobOpening.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateJobOpening(Guid id, [FromBody] UpdateJobOpeningDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.SalaryMin.HasValue && request.SalaryMax.HasValue && request.SalaryMin > request.SalaryMax)
            {
                ModelState.AddModelError(nameof(request.SalaryMin), "Minimum salary cannot exceed maximum salary.");
                return ValidationProblem(ModelState);
            }

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepo.GetDepartmentByIdAsync(request.DepartmentId.Value);
                if (department == null)
                {
                    return NotFound($"Department {request.DepartmentId} not found.");
                }
            }

            var existing = await _jobOpeningRepo.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            _mapper.Map(request, existing);

            await _jobOpeningRepo.UpdateAsync(existing);
            await _jobOpeningRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteJobOpening(Guid id)
        {
            var deleted = await _jobOpeningRepo.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _jobOpeningRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
