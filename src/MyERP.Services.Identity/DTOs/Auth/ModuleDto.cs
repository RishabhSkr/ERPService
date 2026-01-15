namespace MyERP.Services.Identity.DTOs.Auth
{
    public class ModuleDto
    {
        public Guid ModuleId { get; set; } // GUID to match Entity
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool HasAccess { get; set; }
    }
}
