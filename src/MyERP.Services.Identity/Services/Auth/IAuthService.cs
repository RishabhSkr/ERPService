using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.DTOs.Users;
public interface IAuthService
{
    Task<string> RegisterAsync(CreateUserDto request); // Returns UserId
    Task<TokenResponseDto> LoginAsync(LoginRequestDto request, string ipAddress);
}