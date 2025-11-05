using Microsoft.AspNetCore.Mvc;
using UserManagementService.Dtos;
using UserManagementService.Services;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetRoles(CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetRolesAsync(cancellationToken);
            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoleResponseDto>> GetRole(int id, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleAsync(id, cancellationToken);
            return role == null ? NotFound() : Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<RoleResponseDto>> CreateRole([FromBody] RoleCreateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var role = await _roleService.CreateRoleAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(request.Name), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<RoleResponseDto>> UpdateRole(int id, [FromBody] RoleUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var updated = await _roleService.UpdateRoleAsync(id, request, cancellationToken);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(request.Name), ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _roleService.DeleteRoleAsync(id, cancellationToken);
                return deleted ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}/permissions")]
        public async Task<ActionResult<RoleResponseDto>> UpdateRolePermissions(int id, [FromBody] RolePermissionsUpdateDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var role = await _roleService.UpdateRolePermissionsAsync(id, request, cancellationToken);
                return role == null ? NotFound() : Ok(role);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(request.PermissionIds), ex.Message);
                return ValidationProblem(ModelState);
            }
        }
    }
}
