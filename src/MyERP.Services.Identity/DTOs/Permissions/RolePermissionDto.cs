namespace MyERP.Services.Identity.DTOs.Permissions
{
    public class RolePermissionDto
    {
        public Guid RolePermissionId { get; set; } 
        
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        
        public string PermissionName { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        
        public bool IsGranted { get; set; }
    }
}
