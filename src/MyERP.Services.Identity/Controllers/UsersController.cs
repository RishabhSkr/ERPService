using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Identity.DTOs.Users;
using MyERP.Services.Identity.Services.Users;
using MyERP.Services.Identity.Exceptions;
using MyERP.Services.Identity.DTOs;

namespace MyERP.Services.Identity.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // Require login for all user management
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto request)
        {
            var userId = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = userId }, new { UserId = userId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto request)
        {
            if (id != request.UserId) throw new AppException("ID mismatch");

            await _userService.UpdateUserAsync(request);
            return Ok(ApiResponse.Ok("User updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return Ok(ApiResponse.Ok("User deleted successfully"));
        }
    }
}
