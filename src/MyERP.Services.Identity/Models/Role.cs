using System.ComponentModel.DataAnnotations;

namespace MyERP.Services.Identity.Models
{
    public class Role
    {
        public Guid Id { get; set; }

        public string RoleName { get; set; }=string.Empty; // "Admin", "SalesUser"

        public string Description { get; set; }=string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsSystemRole { get; set; } = false; // true = cannot be deleted/edited

        // Navigation (Reverse relationship)
        public ICollection<User>? Users { get; set; }
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}