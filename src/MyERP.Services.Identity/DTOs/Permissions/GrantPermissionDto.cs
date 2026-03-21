namespace MyERP.Services.Identity.DTOs.Permissions
{
    public class GrantPermissionDto
    {
        public Guid RoleId { get; set; } // GUID
        public Guid ModuleId { get; set; }
        
        public string PermissionName { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        
        public bool IsGranted { get; set; } 
    }
}
