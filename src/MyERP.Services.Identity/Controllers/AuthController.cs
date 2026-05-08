using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Identity.DTOs.Roles;
using MyERP.Services.Identity.DTOs;
using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.DTOs.Users;
using MyERP.Services.Identity.Repositories;
using MyERP.Services.Identity.Services.Users;
using System.Security.Claims;
using MyERP.Services.Identity.Services.Roles;
using Microsoft.AspNetCore.Authorization;

namespace MyERP.Services.Identity.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public AuthController(IAuthService authService, IUserService userService, IRoleService roleService)
        {
            _authService = authService;
            _userService = userService;
            _roleService = roleService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var result = await _authService.LoginAsync(request, ipAddress);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Login successful"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto request)
        {
            var userId = await _authService.RegisterAsync(request);
            // return CreatedAtAction(nameof(Login), new { username = request.Username }, new { UserId = userId });
            return Ok(ApiResponse<object>.Ok(new { UserId = userId }, "User registered successfully"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.Fail("User not identified"));
            }
            var userId = Guid.Parse(userIdClaim);
            var userDto = await _userService.GetUserByIdAsync(userId);
            return Ok(ApiResponse<UserDto>.Ok(userDto));
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto request)
        {
            var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.Fail("User not identified"));
            }
            var userId = Guid.Parse(userIdClaim);
            
            // Only allow user to update their own profile
            request.UserId = userId;
            // Don't allow role/status changes through self-update
            request.RoleId = null;
            request.IsActive = null;
            
            await _userService.UpdateUserAsync(request);
            return Ok(ApiResponse.Ok("Profile updated successfully"));
        }

        [HttpGet("roles/public")]
        public async Task<IActionResult> GetPublicRoles()
        {
            var roles = await _roleService.GetPublicRolesAsync();
            return Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(roles));
        }
    }
}
