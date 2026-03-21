namespace MyERP.Services.Inventory.DTOs.Categories
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public int RawMaterialCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
