namespace MyERP.Services.Production.Services.External
{
    public interface IInventoryServiceClient
    {
        Task<RawMaterialDto?> GetRawMaterialByIdAsync(Guid materialId);
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        Task<bool> CheckStockAvailableAsync(Guid materialId, decimal quantity);
    }
}