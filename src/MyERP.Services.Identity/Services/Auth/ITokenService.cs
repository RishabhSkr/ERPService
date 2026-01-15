using UserEntity = MyERP.Services.Identity.Models.User;

namespace MyERP.Services.Identity.Services.Auth
{
    public interface ITokenService
    {
        string GenerateToken(UserEntity user, string roleName);
    }

}