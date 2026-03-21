namespace MyERP.Services.Inventory.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
