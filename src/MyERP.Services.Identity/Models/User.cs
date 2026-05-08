using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyERP.Services.Identity.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty; 

        public string Email { get; set; } = string.Empty;   
        public string PasswordHash { get; set; }= string.Empty; 

        // Foreign Key for Role (Master Data)
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }

        public Guid? RequestedRoleId { get; set; } // Tracks what role user asked for during signup

        // Audit Fields
        public string Status { get; set; } = SystemConstants.StatusPending;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}