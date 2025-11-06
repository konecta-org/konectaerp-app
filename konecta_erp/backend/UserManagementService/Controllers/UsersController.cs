using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedContracts.Authorization;
using SharedContracts.Configuration;
using UserManagementService.Dtos;
using UserManagementService.Services;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;
        private readonly ServiceAuthOptions _serviceAuthOptions;

        public UsersController(IUserService userService, IMapper mapper, ILogger<UsersController> logger, IOptions<ServiceAuthOptions> serviceAuthOptions)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
            _serviceAuthOptions = serviceAuthOptions.Value;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<UserResponseDto>>> GetUsers([FromQuery] UserQueryParameters parameters, CancellationToken cancellationToken)
        {
            var pagedUsers = await _userService.GetUsersAsync(parameters, cancellationToken);
            var mappedUsers = pagedUsers.Items.Select(user => _mapper.Map<UserResponseDto>(user)).ToList();
            var response = new PagedResultDto<UserResponseDto>(mappedUsers, pagedUsers.Page, pagedUsers.PageSize, pagedUsers.TotalItems);
            return Ok(response);
        }
        [HttpGet("all-users")]
        public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers( CancellationToken cancellationToken)
        {
           var users = await _userService.GetAllUsersAsync(cancellationToken);
           var mappedUsers = users.Select(user => _mapper.Map<UserResponseDto>(user)).ToList();
           return Ok(mappedUsers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(string id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UserResponseDto>(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserCreateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var user = await _userService.CreateAsync(request, cancellationToken);
                var response = _mapper.Map<UserResponseDto>(user);
                return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create user {Email}", request.Email);
                ModelState.AddModelError(nameof(request.Email), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(string id, [FromBody] UserUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var updated = await _userService.UpdateAsync(id, request, cancellationToken);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UserResponseDto>(updated));
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] RoleChangeDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var changed = await _userService.ChangeRoleAsync(id, request, cancellationToken);
            return changed ? NoContent() : NotFound();
        }

        [HttpGet("{id}/roles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetUserRoles(string id, CancellationToken cancellationToken)
        {
            var roles = await _userService.GetUserRolesAsync(id, cancellationToken);
            if (roles.Count == 0)
            {
                var user = await _userService.GetByIdAsync(id, cancellationToken);
                if (user == null || user.IsDeleted)
                {
                    return NotFound();
                }
            }

            return Ok(roles);
        }

        [HttpPut("{id}/roles")]
        public async Task<IActionResult> SetUserRoles(string id, [FromBody] UserRoleAssignmentDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var updated = await _userService.SetUserRolesAsync(id, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UserStatusUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var updated = await _userService.UpdateStatusAsync(id, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(string id, CancellationToken cancellationToken)
        {
            var deleted = await _userService.SoftDeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(string id, CancellationToken cancellationToken)
        {
            var restored = await _userService.RestoreAsync(id, cancellationToken);
            return restored ? NoContent() : NotFound();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<UserSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            var summary = await _userService.GetSummaryAsync(cancellationToken);
            return Ok(summary);
        }

        [HttpGet("{id}/authorizations")]
        [AllowAnonymous]
        public async Task<ActionResult<UserAuthorizationProfileDto>> GetAuthorizationProfile(string id, CancellationToken cancellationToken)
        {
            if (!Request.Headers.TryGetValue("X-Service-Token", out var tokenHeader))
            {
                return Unauthorized();
            }

            var providedToken = tokenHeader.FirstOrDefault();
            if (!IsServiceTokenValid(providedToken))
            {
                return Unauthorized();
            }

            var profile = await _userService.GetAuthorizationProfileAsync(id, cancellationToken);
            return profile == null ? NotFound() : Ok(profile);
        }

        private bool IsServiceTokenValid(string? providedToken)
        {
            if (string.IsNullOrWhiteSpace(_serviceAuthOptions.SharedSecret))
            {
                _logger.LogWarning("Service-to-service shared secret is not configured.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(providedToken))
            {
                return false;
            }

            var providedBytes = System.Text.Encoding.UTF8.GetBytes(providedToken);
            var expectedBytes = System.Text.Encoding.UTF8.GetBytes(_serviceAuthOptions.SharedSecret);
            return providedBytes.Length == expectedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
        }
    }
}
