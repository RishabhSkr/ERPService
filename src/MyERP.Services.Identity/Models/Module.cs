using System.ComponentModel.DataAnnotations;

namespace MyERP.Services.Identity.Models
{
    public class Module
    {
        public Guid Id { get; set; }

        public string ModuleName { get; set; }=string.Empty;
        
        public string ModuleCode { get; set; }=string.Empty; 

        public int DisplayOrder { get; set; } 
        public bool IsActive { get; set; } = true;

        public ICollection<RolePermission>? Permissions { get; set; }
    }
}