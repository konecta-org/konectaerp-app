using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Dtos;
using UserManagementService.Services;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public PermissionsController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionResponseDto>>> GetPermissions(CancellationToken cancellationToken)
        {
            var permissions = await _roleService.GetPermissionsAsync(cancellationToken);
            return Ok(permissions);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PermissionResponseDto>> GetPermission(int id, CancellationToken cancellationToken)
        {
            var permission = await _roleService.GetPermissionAsync(id, cancellationToken);
            return permission == null ? NotFound() : Ok(permission);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionResponseDto>> CreatePermission([FromBody] PermissionCreateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var permission = await _roleService.CreatePermissionAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, permission);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(request.Name), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PermissionResponseDto>> UpdatePermission(int id, [FromBody] PermissionUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var updated = await _roleService.UpdatePermissionAsync(id, request, cancellationToken);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(request.Name), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePermission(int id, CancellationToken cancellationToken)
        {
            var deleted = await _roleService.DeletePermissionAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
    }
}
