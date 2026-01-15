using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyERP.Services.Identity.Models
{
    public class RolePermission
    {
        public Guid RolePermissionId { get; set; }

        public Guid RoleId { get; set; }
        public Role? Role { get; set; }

        public Guid ModuleId { get; set; }
        public Module? Module { get; set; }

        public string PermissionName { get; set; }=string.Empty; // e.g., "Create Order"
        public string ApiEndpoint { get; set; }=string.Empty;    // e.g., "/api/sales/orders"
        public string HttpMethod { get; set; } = string.Empty;     // e.g., "POST", "GET"

        public bool IsGranted { get; set; } = true;
    }
}