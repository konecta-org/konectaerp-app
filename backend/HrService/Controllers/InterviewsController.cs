using AutoMapper;
using HrService.Dtos;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterviewsController : ControllerBase
    {
        private readonly IInterviewRepo _interviewRepo;
        private readonly IJobApplicationRepo _jobApplicationRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IMapper _mapper;

        public InterviewsController(
            IInterviewRepo interviewRepo,
            IJobApplicationRepo jobApplicationRepo,
            IEmployeeRepo employeeRepo,
            IMapper mapper)
        {
            _interviewRepo = interviewRepo;
            _jobApplicationRepo = jobApplicationRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetInterviews([FromQuery] Guid? jobApplicationId)
        {
            IEnumerable<Models.Interview> interviews;
            if (jobApplicationId.HasValue)
            {
                interviews = await _interviewRepo.GetByApplicationAsync(jobApplicationId.Value);
            }
            else
            {
                interviews = await _interviewRepo.GetAllAsync();
            }

            var response = _mapper.Map<IEnumerable<InterviewResponseDto>>(interviews);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetInterview(Guid id)
        {
            var interview = await _interviewRepo.GetByIdAsync(id);
            if (interview == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<InterviewResponseDto>(interview);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var application = await _jobApplicationRepo.GetByIdAsync(request.JobApplicationId);
            if (application == null)
            {
                return NotFound($"Job application {request.JobApplicationId} not found.");
            }

            if (request.InterviewerEmployeeId.HasValue)
            {
                var interviewer = await _employeeRepo.GetEmployeeByIdAsync(request.InterviewerEmployeeId.Value);
                if (interviewer == null)
                {
                    return NotFound($"Interviewer {request.InterviewerEmployeeId} not found.");
                }
            }

            var interview = _mapper.Map<Models.Interview>(request);
            await _interviewRepo.AddAsync(interview);
            await _interviewRepo.SaveChangesAsync();

            interview = await _interviewRepo.GetByIdAsync(interview.Id);
            var response = _mapper.Map<InterviewResponseDto>(interview);

            return CreatedAtAction(nameof(GetInterview), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateInterview(Guid id, [FromBody] UpdateInterviewDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var interview = await _interviewRepo.GetByIdAsync(id);
            if (interview == null)
            {
                return NotFound();
            }

            if (request.InterviewerEmployeeId.HasValue)
            {
                var interviewer = await _employeeRepo.GetEmployeeByIdAsync(request.InterviewerEmployeeId.Value);
                if (interviewer == null)
                {
                    return NotFound($"Interviewer {request.InterviewerEmployeeId} not found.");
                }
            }

            _mapper.Map(request, interview);

            await _interviewRepo.UpdateAsync(interview);
            await _interviewRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteInterview(Guid id)
        {
            var deleted = await _interviewRepo.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            await _interviewRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
