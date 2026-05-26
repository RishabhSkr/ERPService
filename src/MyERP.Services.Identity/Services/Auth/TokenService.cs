using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserEntity = MyERP.Services.Identity.Models.User;

namespace MyERP.Services.Identity.Services.Auth
{
   
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UserEntity user, string roleName)
        {
            var jwtKey = _config["Jwt:Key"]??"";
            Console.WriteLine($"ðŸ”‘ Identity JWT Key (first 10 chars): {jwtKey.Substring(0, Math.Min(10, jwtKey.Length))}...");
            Console.WriteLine($"ðŸ”‘ Identity JWT Key Length: {jwtKey.Length}");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(JwtRegisteredClaimNames.Email, user.Email),           
                new Claim(ClaimTypes.Role, roleName),                         
                new Claim("Username", user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique Token ID
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]??"")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
