using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.DTOs.Users;

namespace MyERP.Services.Identity.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var result = await _authService.LoginAsync(request, ipAddress);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto request)
        {
            var userId = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Login), new { username = request.Username }, new { UserId = userId });
        }
    }
}
