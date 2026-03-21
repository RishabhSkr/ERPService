using System;

namespace MyERP.Services.Identity.DTOs.Roles
{
    public class RoleDto
    {
        public Guid RoleId { get; set; } // GUID
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        
        // Computed Properties
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
