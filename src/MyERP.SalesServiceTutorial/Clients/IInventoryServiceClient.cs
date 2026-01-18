using System.Text.Json.Serialization;

namespace MyERP.SalesServiceTutorial.Clients;
    
public class ProductDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("productName")]  // ← Match Inventory's field
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("totalStock")]  // ← Match Inventory's field
    public decimal Stock { get; set; }
}
public interface IInventoryServiceClient
{
    Task<ProductDto> GetProductByIdAsync(Guid id);
}