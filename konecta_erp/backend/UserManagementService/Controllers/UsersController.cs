using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Dtos;
using UserManagementService.Models;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext db, ILogger<UsersController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _db.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserResponseDto {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Department = u.Department,
                    Role = u.Role,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(string id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null || u.IsDeleted) return NotFound();

            var dto = new UserResponseDto {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Department = u.Department,
                Role = u.Role,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt
            };
            return Ok(dto);
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] RoleChangeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewRole)) return BadRequest("NewRole is required.");

            var user = await _db.Users.FindAsync(id);
            if (user == null || user.IsDeleted) return NotFound();

            user.Role = dto.NewRole;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsDeleted = true;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UserResponseDto dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var user = await _db.Users.FindAsync(id);
            if (user == null || user.IsDeleted) return NotFound();

            user.FullName = dto.FullName;
            user.Department = dto.Department;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
