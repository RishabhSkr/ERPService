using System.Text.Json;
using MyERP.Services.Sales.DTOs;

namespace MyERP.Services.Sales.Services.External
{
    public interface IInventoryServiceClient
    {
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        Task<bool> ProductExistsAsync(Guid productId);
        Task<List<ProductDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
    }

    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/inventory/products/{productId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Product {ProductId} not found in Inventory Service", productId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<InventoryApiResponse<ProductDto>>(json, _jsonOptions);
                
                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId} from Inventory Service", productId);
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(Guid productId)
        {
            var product = await GetProductByIdAsync(productId);
            return product != null && product.IsActive;
        }

        public async Task<List<ProductDto>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
        {
            var products = new List<ProductDto>();
            
            foreach (var productId in productIds)
            {
                var product = await GetProductByIdAsync(productId);
                if (product != null)
                    products.Add(product);
            }
            
            return products;
        }
    }

    // Internal class for deserializing Inventory API response
    internal class InventoryApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
