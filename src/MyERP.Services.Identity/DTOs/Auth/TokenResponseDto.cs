using System.Collections.Generic;

namespace MyERP.Services.Identity.DTOs.Auth
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        
        public Guid UserId { get; set; } 
        public string Username { get; set; } = string.Empty;
        
        public Guid RoleId { get; set; } // GUID to match Entity
        public string RoleName { get; set; } = string.Empty;
        
        public int AccessTokenExpiresIn { get; set; } // Seconds
        public int RefreshTokenExpiresIn { get; set; } // Seconds
        
        public List<ModuleDto> AccessibleModules { get; set; } = new();
    }
}
