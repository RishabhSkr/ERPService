using System;

namespace MyERP.Services.Identity.DTOs.Users
{
    public class UserDto
    {
        public Guid UserId { get; set; } 
        
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        public Guid RoleId { get; set; } // GUID
        public string RoleName { get; set; } = string.Empty;
        
        public Guid? RequestedRoleId { get; set; }
        public string RequestedRoleName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
