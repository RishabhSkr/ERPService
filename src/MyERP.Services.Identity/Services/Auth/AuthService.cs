using System.Security.Cryptography;
using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.DTOs.Users;
using MyERP.Services.Identity.Models;
using MyERP.Services.Identity.Repositories;
using MyERP.Services.Identity.Services.Auth;
using MyERP.Services.Identity.Exceptions;

namespace MyERP.Services.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepo, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
        }

        // Login Logic
        public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
        {
            // 1. Get User
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null) // Don't check password here to avoid timing attacks? Actually standard is verify.
            {
                 // Dummy verify to simulate time if needed, but for now simple check.
            }
             if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid Credentials");

            // 2. Generate Access Token
            string roleName = user.Role?.RoleName ?? "User";
            string accessToken = _tokenService.GenerateToken(user, roleName);

            // 3. Generate Refresh Token
            var refreshToken = CreateRefreshToken(user.Id, ipAddress);
            await _userRepo.SaveRefreshTokenAsync(refreshToken);

            // 4. Get UI Modules
            var modules = await _userRepo.GetAccessibleModulesAsync(user.RoleId);

            // 5. Return Full Response
            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                UserId = user.Id,
                Username = user.Username,
                RoleName = roleName,
                RoleId = user.RoleId,
                // AccessTokenExpiresIn = 900, // Config se lena chahiye
                AccessibleModules = modules
            };
        }

        // Register Logic
        public async Task<string> RegisterAsync(CreateUserDto request)
        {
            if (await _userRepo.ExistsAsync(request.Email))
                throw new AppException("Email exists");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId, // Now a Guid
                IsActive = true
            };

            await _userRepo.CreateAsync(newUser);
            return newUser.Id.ToString();
        }

        // Helper: Random Token Generator
        private RefreshToken CreateRefreshToken(Guid userId, string ipAddress)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Convert.ToBase64String(randomBytes),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
    }
}