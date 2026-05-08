using MyERP.Services.Identity.DTOs.Users;

namespace MyERP.Services.Identity.Services.Users
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task UpdateUserAsync(UpdateUserDto request);
        Task DeleteUserAsync(Guid userId);
        Task<Guid> CreateUserAsync(CreateUserDto request);
        Task ApproveUserAsync(Guid userId, Guid roleId);
        Task SuspendUserAsync(Guid userId);
    }
}
