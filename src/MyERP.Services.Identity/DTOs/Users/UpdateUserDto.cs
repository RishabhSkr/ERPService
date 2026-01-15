namespace MyERP.Services.Identity.DTOs.Users
{
    public class UpdateUserDto
    {
        public Guid UserId { get; set; } 
        
        public string Email { get; set; } = string.Empty;
        public Guid? RoleId { get; set; } // GUID
        public bool? IsActive { get; set; }
    }
}
