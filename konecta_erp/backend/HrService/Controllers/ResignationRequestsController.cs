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
    public class ResignationRequestsController : ControllerBase
    {
        private readonly IResignationRequestRepo _resignationRepo;
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly RabbitMqOptions _rabbitOptions;
        private readonly ILogger<ResignationRequestsController> _logger;

        public ResignationRequestsController(
            IResignationRequestRepo resignationRepo,
            IEmployeeRepo employeeRepo,
            IMapper mapper,
            IEventPublisher eventPublisher,
            IOptions<RabbitMqOptions> rabbitOptions,
            ILogger<ResignationRequestsController> logger)
        {
            _resignationRepo = resignationRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _rabbitOptions = rabbitOptions.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResignation([FromBody] SubmitResignationRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var employee = await _employeeRepo.GetEmployeeByIdAsync(requestDto.EmployeeId, includeDepartment: false);
            if (employee == null)
            {
                return NotFound($"Employee {requestDto.EmployeeId} not found.");
            }

            if (employee.Status is EmploymentStatus.Resigned or EmploymentStatus.Terminated)
            {
                return Conflict("Employee already has an exit status.");
            }

            if (await _resignationRepo.HasPendingRequestAsync(requestDto.EmployeeId))
            {
                return Conflict("Employee already has a pending resignation request.");
            }

            if (requestDto.EffectiveDate < employee.HireDate)
            {
                ModelState.AddModelError(nameof(requestDto.EffectiveDate), "Effective date cannot be earlier than hire date.");
                return ValidationProblem(ModelState);
            }

            var resignation = _mapper.Map<ResignationRequest>(requestDto);
            resignation.Employee = employee;

            await _resignationRepo.AddAsync(resignation);
            await _resignationRepo.SaveChangesAsync();

            var response = _mapper.Map<ResignationRequestResponseDto>(resignation);
            return CreatedAtAction(nameof(GetResignationById), new { id = resignation.Id }, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetResignations([FromQuery] ResignationStatus? status)
        {
            var resignations = await _resignationRepo.GetAllAsync(status);
            var response = _mapper.Map<IEnumerable<ResignationRequestResponseDto>>(resignations);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetResignationById(Guid id)
        {
            var resignation = await _resignationRepo.GetByIdAsync(id, includeEmployee: true);
            if (resignation == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<ResignationRequestResponseDto>(resignation);
            return Ok(response);
        }

        [HttpPut("{id:guid}/decision")]
        public async Task<IActionResult> DecideResignation(Guid id, [FromBody] ReviewResignationRequestDto requestDto, CancellationToken cancellationToken)
        {
            if (id != requestDto.Id)
            {
                return BadRequest("Route id and payload id do not match.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (requestDto.Decision == ResignationStatus.Pending)
            {
                ModelState.AddModelError(nameof(requestDto.Decision), "Decision must be Approved or Rejected.");
                return ValidationProblem(ModelState);
            }

            var resignation = await _resignationRepo.GetByIdAsync(id, includeEmployee: true);
            if (resignation == null)
            {
                return NotFound();
            }

            if (resignation.Status != ResignationStatus.Pending)
            {
                return Conflict("Resignation request has already been processed.");
            }

            var employee = resignation.Employee ?? await _employeeRepo.GetEmployeeByIdAsync(resignation.EmployeeId, includeDepartment: false);
            if (employee == null)
            {
                return NotFound($"Employee {resignation.EmployeeId} not found.");
            }

            if (requestDto.Decision == ResignationStatus.Approved)
            {
                resignation.Status = ResignationStatus.Approved;
                resignation.DecidedAt = DateTime.UtcNow;
                resignation.DecisionNotes = requestDto.DecisionNotes;
                resignation.ApprovedByEmployeeId = requestDto.ApprovedByEmployeeId;

                var eligibleForRehire = requestDto.EligibleForRehire ?? false;
                resignation.EligibleForRehire = eligibleForRehire;

                var exitRecorded = await _employeeRepo.RecordEmployeeExitAsync(employee.Id, resignation.EffectiveDate, EmploymentStatus.Resigned, resignation.Reason, eligibleForRehire);
                if (!exitRecorded)
                {
                    return NotFound($"Unable to apply exit details for employee {employee.Id}.");
                }

                await _resignationRepo.UpdateAsync(resignation);
                await _resignationRepo.SaveChangesAsync();

                var exitEvent = new EmployeeResignationApprovedEvent(
                    resignation.Id,
                    employee.Id,
                    employee.UserId,
                    resignation.EffectiveDate,
                    resignation.Reason,
                    resignation.DecidedAt ?? DateTime.UtcNow);

                await _eventPublisher.PublishAsync(_rabbitOptions.EmployeeResignationApprovedRoutingKey, exitEvent, cancellationToken);
                _logger.LogInformation("Resignation request {ResignationId} approved for employee {EmployeeId}.", resignation.Id, employee.Id);

                var response = _mapper.Map<ResignationRequestResponseDto>(resignation);
                return Ok(response);
            }
            else
            {
                resignation.Status = ResignationStatus.Rejected;
                resignation.DecidedAt = DateTime.UtcNow;
                resignation.DecisionNotes = requestDto.DecisionNotes;
                resignation.ApprovedByEmployeeId = requestDto.ApprovedByEmployeeId;
                resignation.EligibleForRehire = null;

                await _resignationRepo.UpdateAsync(resignation);
                await _resignationRepo.SaveChangesAsync();

                var response = _mapper.Map<ResignationRequestResponseDto>(resignation);
                return Ok(response);
            }
        }
    }
}

