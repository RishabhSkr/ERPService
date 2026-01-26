namespace MyERP.Services.Identity.DTOs.Permissions;
public class CheckPermissionDto
{
    public string RoleName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
}
public class CheckPermissionResultDto
{
    public bool HasAccess { get; set; }
}