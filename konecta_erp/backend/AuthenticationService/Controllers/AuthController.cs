using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationService.Dtos;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new GenericResponse
                {
                    Result = null,
                    Code = "400",
                    C_Message = "Email already exists.",
                    S_Message = "Attempt to register an existing email."
                });

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(new GenericResponse
                {
                    Result = result.Errors,
                    Code = "400",
                    C_Message = "Registration failed. Please check your input.",
                    S_Message = string.Join("; ", result.Errors.Select(e => e.Description))
                });

            return Ok(new GenericResponse
            {
                Result = new { user.FullName, user.Email },
                Code = "200",
                C_Message = "User registered successfully!",
                S_Message = "User created in Identity successfully."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new GenericResponse
                {
                    Result = null,
                    Code = "401",
                    C_Message = "Invalid email or password.",
                    S_Message = "User not found during login."
                });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new GenericResponse
                {
                    Result = null,
                    Code = "401",
                    C_Message = "Invalid email or password.",
                    S_Message = "Password verification failed."
                });

            var token = _jwtService.GenerateToken(user);

            return Ok(new GenericResponse
            {
                Result = new { Token = token, UserId = user.Id, user.Email },
                Code = "200",
                C_Message = "Login successful.",
                S_Message = "JWT token generated successfully."
            });
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
                return Unauthorized(new GenericResponse
                {
                    Result = null,
                    Code = "401",
                    C_Message = "Invalid or expired token.",
                    S_Message = "JWT validation failed."
                });

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new GenericResponse
            {
                Result = new { Email = email },
                Code = "200",
                C_Message = "Token is valid.",
                S_Message = "JWT token successfully validated."
            });
        }

        [HttpPut("update-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (email == null)
                return Unauthorized(new GenericResponse
                {
                    Result = null,
                    Code = "401",
                    C_Message = "Invalid token.",
                    S_Message = "Token does not contain a valid email claim."
                });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new GenericResponse
                {
                    Result = null,
                    Code = "404",
                    C_Message = "User not found.",
                    S_Message = $"No user found for email: {email}."
                });

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, request.OldPassword, false);
            if (!passwordCheck.Succeeded)
                return BadRequest(new GenericResponse
                {
                    Result = null,
                    Code = "400",
                    C_Message = "Old password is incorrect.",
                    S_Message = "Password check failed during update."
                });

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new GenericResponse
                {
                    Result = null,
                    Code = "400",
                    C_Message = "New password and confirmation do not match.",
                    S_Message = "Password confirmation mismatch."
                });

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new GenericResponse
                {
                    Result = result.Errors,
                    Code = "400",
                    C_Message = "Password update failed.",
                    S_Message = string.Join("; ", result.Errors.Select(e => e.Description))
                });

            return Ok(new GenericResponse
            {
                Result = new { user.Email },
                Code = "200",
                C_Message = "Password updated successfully.",
                S_Message = "User password changed successfully in Identity."
            });
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            return Ok(new GenericResponse
            {
                Result = new { Email = email, Claims = claims },
                Code = "200",
                C_Message = "User data retrieved successfully.",
                S_Message = "Fetched claims and email from authenticated token."
            });
        }
    }
}
