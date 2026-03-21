namespace MyERP.Services.Production.Services.External
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// RawMaterial DTO from Inventory Service
    /// Maps Inventory fields: TotalStock → CurrentStock, TotalReserved → ReservedStock, etc.
    /// </summary>
    public class RawMaterialDto
    {
        public Guid Id { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public object? Unit { get; set; }  // Inventory returns unit as object {id, name, symbol}

        [JsonPropertyName("totalStock")]
        public decimal CurrentStock { get; set; }

        [JsonPropertyName("totalReserved")]
        public decimal ReservedStock { get; set; }

        [JsonPropertyName("totalAvailable")]
        public decimal AvailableStock { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Product DTO from Inventory Service
    /// </summary>
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    // Internal response wrapper
    internal class InventoryApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}