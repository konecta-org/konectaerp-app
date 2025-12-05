using AutoMapper;
using HrService.Dtos;
using HrService.Models;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobApplicationsController : ControllerBase
    {
        private readonly IJobApplicationRepo _applicationRepo;
        private readonly IJobOpeningRepo _jobOpeningRepo;
        private readonly IMapper _mapper;

        public JobApplicationsController(
            IJobApplicationRepo applicationRepo,
            IJobOpeningRepo jobOpeningRepo,
            IMapper mapper)
        {
            _applicationRepo = applicationRepo;
            _jobOpeningRepo = jobOpeningRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetApplications([FromQuery] Guid? jobOpeningId)
        {
            IEnumerable<JobApplication> applications;
            if (jobOpeningId.HasValue)
            {
                applications = await _applicationRepo.GetByJobOpeningAsync(jobOpeningId.Value);
            }
            else
            {
                applications = await _applicationRepo.GetAllAsync();
            }

            var response = _mapper.Map<IEnumerable<JobApplicationResponseDto>>(applications);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetApplication(Guid id)
        {
            var application = await _applicationRepo.GetByIdAsync(id, includeDetails: true);
            if (application == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<JobApplicationResponseDto>(application);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromBody] CreateJobApplicationDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var jobOpening = await _jobOpeningRepo.GetByIdAsync(request.JobOpeningId, includeApplications: true);
            if (jobOpening == null)
            {
                return NotFound($"Job opening {request.JobOpeningId} not found.");
            }

            var duplicate = jobOpening.Applications?.Any(a => a.CandidateEmail == request.CandidateEmail) ?? false;
            if (duplicate)
            {
                return Conflict("This candidate has already applied for the job opening.");
            }

            var application = _mapper.Map<JobApplication>(request);
            await _applicationRepo.AddAsync(application);
            await _applicationRepo.SaveChangesAsync();

            application = await _applicationRepo.GetByIdAsync(application.Id);
            var response = _mapper.Map<JobApplicationResponseDto>(application);

            return CreatedAtAction(nameof(GetApplication), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] UpdateJobApplicationDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var application = await _applicationRepo.GetByIdAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            _mapper.Map(request, application);

            await _applicationRepo.UpdateAsync(application);
            await _applicationRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            var deleted = await _applicationRepo.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _applicationRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
