using MyERP.Services.Identity.DTOs.Users;
using MyERP.Services.Identity.Models;
using MyERP.Services.Identity.Repositories;
using MyERP.Services.Identity.Exceptions;

namespace MyERP.Services.Identity.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            
            return users.Select(u => new UserDto
            {
                UserId = u.Id,
                Username = u.Username,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName ?? "Unknown",
                RequestedRoleId = u.RequestedRoleId,
                Status = u.Status,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            return new UserDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "Unknown",
                RequestedRoleId = user.RequestedRoleId,
                Status = user.Status,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<Guid> CreateUserAsync(CreateUserDto request)
        {
            // Reuse repository check
            if (await _userRepo.ExistsAsync(request.Email))
                throw new AppException("Email already exists");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsActive = true
            };

            await _userRepo.CreateAsync(newUser);
            return newUser.Id;
        }

        public async Task UpdateUserAsync(UpdateUserDto request)
        {
            var user = await _userRepo.GetByIdAsync(request.UserId);
            if (user == null) throw new NotFoundException("User not found");

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                 if (await _userRepo.ExistsAsync(request.Email))
                    throw new AppException("Email already exists");
                 user.Email = request.Email;
            }

            if (request.RoleId.HasValue)
            {
                user.RoleId = request.RoleId.Value;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            await _userRepo.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            await _userRepo.DeleteAsync(userId);
        }

        public async Task ApproveUserAsync(Guid userId, Guid roleId)
        {
            await _userRepo.ApproveUserAsync(userId, roleId);
        }

        public async Task SuspendUserAsync(Guid userId)
        {
            await _userRepo.SuspendUserAsync(userId);
        }
    }
}
