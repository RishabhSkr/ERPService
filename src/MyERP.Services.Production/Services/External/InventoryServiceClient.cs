using System.Text.Json;

namespace MyERP.Services.Production.Services.External
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public InventoryServiceClient(
            HttpClient httpClient,
            ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<RawMaterialDto?> GetRawMaterialByIdAsync(Guid materialId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/inventory/raw-materials/{materialId}");
                Console.WriteLine($"Response: {response}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Material {MaterialId} not found", materialId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<InventoryApiResponse<RawMaterialDto>>(
                    json, _jsonOptions);
                
                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching material {MaterialId}", materialId);
                throw new ApplicationException($"Failed to fetch material {materialId}", ex);
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/inventory/products/{productId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Product {ProductId} not found", productId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<InventoryApiResponse<ProductDto>>(
                    json, _jsonOptions);
                
                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", productId);
                throw new ApplicationException($"Failed to fetch product {productId}", ex);
            }
        }

        public async Task<bool> CheckStockAvailableAsync(Guid materialId, decimal quantity)
        {
            var material = await GetRawMaterialByIdAsync(materialId);
            return material != null && material.AvailableStock >= quantity;
        }
    }
}