using MyERP.SalesServiceTutorial.Common.Responses;
using System.Text.Json.Serialization;

namespace MyERP.SalesServiceTutorial.Clients;

// Wrapper for Inventory Service response (camelCase)
public class InventoryApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;
    
    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/inventory/products/{id}");
        response.EnsureSuccessStatusCode();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<InventoryApiResponse<ProductDto>>();
        return apiResponse?.Data;
    }
}