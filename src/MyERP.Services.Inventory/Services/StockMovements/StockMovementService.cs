using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Inventory;
using MyERP.Services.Inventory.Repositories.Products;
using MyERP.Services.Inventory.Repositories.RawMaterials;
using MyERP.Services.Inventory.Repositories.Warehouses;

namespace MyERP.Services.Inventory.Services.StockMovements
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRawMaterialRepository _rawMaterialRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public StockMovementService(
            IStockMovementRepository stockMovementRepository,
            IProductRepository productRepository,
            IRawMaterialRepository rawMaterialRepository,
            IWarehouseRepository warehouseRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _productRepository = productRepository;
            _rawMaterialRepository = rawMaterialRepository;
            _warehouseRepository = warehouseRepository;
        }

        public async Task<StockMovementResponseDto> RecordMovementAsync(RecordStockMovementDto dto, Guid? userId = null)
        {
            // Validate warehouse exists
            var warehouseExists = await _warehouseRepository.ExistsAsync(dto.WarehouseId);
            if (!warehouseExists)
                throw new NotFoundException("Warehouse", dto.WarehouseId);

            string itemCode;
            string itemName;
            decimal stockBefore;
            decimal stockAfter;

            if (dto.ItemType.ToLower() == "product")
            {
                var product = await _productRepository.GetByIdAsync(dto.ItemId);
                if (product == null)
                    throw new NotFoundException("Product", dto.ItemId);

                itemCode = product.ProductCode;
                itemName = product.ProductName;

                // Get or create inventory record
                var inventory = await _productRepository.GetInventoryAsync(dto.ItemId, dto.WarehouseId);

                if (inventory == null)
                {
                    inventory = new ProductInventory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = dto.ItemId,
                        WarehouseId = dto.WarehouseId,
                        CurrentStock = 0,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _productRepository.AddInventoryAsync(inventory);
                }

                stockBefore = inventory.CurrentStock;

                // Apply movement
                switch (dto.MovementType.ToUpper())
                {
                    case "IN":
                        inventory.CurrentStock += dto.Quantity;
                        break;
                    case "OUT":
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    case "RESERVE":
                        inventory.ReservedStock += dto.Quantity;
                        break;
                    case "RELEASE":
                        inventory.ReservedStock -= dto.Quantity;
                        break;
                    case "ADJUST":
                        inventory.CurrentStock = dto.Quantity;
                        break;
                    default:
                        throw new BadRequestException($"Invalid movement type: {dto.MovementType}");
                }

                stockAfter = inventory.CurrentStock;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateInventoryAsync(inventory);
            }
            else if (dto.ItemType.ToLower() == "rawmaterial")
            {
                var rawMaterial = await _rawMaterialRepository.GetByIdAsync(dto.ItemId);
                if (rawMaterial == null)
                    throw new NotFoundException("RawMaterial", dto.ItemId);

                itemCode = rawMaterial.MaterialCode;
                itemName = rawMaterial.MaterialName;

                // Get or create inventory record
                var inventory = await _rawMaterialRepository.GetInventoryAsync(dto.ItemId, dto.WarehouseId);

                // Fallback: if batch didn't match, find ANY existing record for this material+warehouse
                if (inventory == null)
                {
                    var allInventories = await _rawMaterialRepository.GetInventoriesAsync(dto.ItemId);
                    inventory = allInventories.FirstOrDefault(i => i.WarehouseId == dto.WarehouseId);
                }

                if (inventory == null)
                {
                    // Truly new — no record exists at all
                    inventory = new RawMaterialInventory
                    {
                        Id = Guid.NewGuid(),
                        RawMaterialId = dto.ItemId,
                        WarehouseId = dto.WarehouseId,
                        CurrentStock = 0,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _rawMaterialRepository.AddInventoryAsync(inventory);
                }

                stockBefore = inventory.CurrentStock;

                // Apply movement
                switch (dto.MovementType.ToUpper())
                {
                    case "IN":
                        inventory.CurrentStock += dto.Quantity;
                        break;
                    case "OUT":
                        if (inventory.CurrentStock < dto.Quantity)
                            throw new BadRequestException(
                                $"Insufficient stock. Need {dto.Quantity}, current {inventory.CurrentStock}");
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    case "RESERVE":
                        // Check: AvailableStock (CurrentStock - ReservedStock) >= Quantity?
                        var available = inventory.CurrentStock - inventory.ReservedStock;
                        if (available < dto.Quantity)
                            throw new BadRequestException(
                                $"Insufficient available stock. Need {dto.Quantity}, available {available}");
                        inventory.ReservedStock += dto.Quantity;
                        break;
                    case "RELEASE":
                        inventory.ReservedStock -= dto.Quantity;
                        break;
                    case "ADJUST":
                        inventory.CurrentStock = dto.Quantity;
                        break;
                    case "SCRAP":
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    default:
                        throw new BadRequestException($"Invalid movement type: {dto.MovementType}");
                }

                stockAfter = inventory.CurrentStock;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _rawMaterialRepository.UpdateInventoryAsync(inventory);
            }
            else
            {
                throw new BadRequestException($"Invalid item type: {dto.ItemType}. Must be 'Product' or 'RawMaterial'");
            }

            // Create stock movement record
            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                ItemType = dto.ItemType,
                ItemId = dto.ItemId,
                ItemCode = itemCode,
                ItemName = itemName,
                WarehouseId = dto.WarehouseId,
                MovementType = dto.MovementType.ToUpper(),
                Quantity = dto.Quantity,
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                ReferenceType = dto.ReferenceType,
                ReferenceId = dto.ReferenceId,
                Notes = dto.Notes,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _stockMovementRepository.AddAsync(movement);

            return MapToDto(movement);
        }

        public async Task<PagedResponse<StockMovementResponseDto>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? itemType = null,
            Guid? itemId = null,
            string? movementType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var (movements, totalCount) = await _stockMovementRepository.GetAllAsync(
                pageNumber, pageSize, itemType, itemId, movementType, startDate, endDate);

            var data = movements.Select(MapToDto).ToList();

            return new PagedResponse<StockMovementResponseDto>(data, pageNumber, pageSize, totalCount);
        }

        public async Task<StockMovementResponseDto> GetByIdAsync(Guid id)
        {
            var movement = await _stockMovementRepository.GetByIdAsync(id);
            if (movement == null)
                throw new NotFoundException("StockMovement", id);

            return MapToDto(movement);
        }

        public async Task<List<StockMovementResponseDto>> GetByItemAsync(string itemType, Guid itemId)
        {
            var movements = await _stockMovementRepository.GetByItemAsync(itemType, itemId);
            return movements.Select(MapToDto).ToList();
        }

        private static StockMovementResponseDto MapToDto(StockMovement movement)
        {
            return new StockMovementResponseDto
            {
                Id = movement.Id,
                MovementType = movement.MovementType,
                ItemType = movement.ItemType,
                ItemId = movement.ItemId,
                ItemCode = movement.ItemCode,
                ItemName = movement.ItemName,
                Quantity = movement.Quantity,
                StockBefore = movement.StockBefore,
                StockAfter = movement.StockAfter,
                ReferenceType = movement.ReferenceType,
                ReferenceId = movement.ReferenceId,
                Notes = movement.Notes,
                CreatedBy = movement.CreatedBy?.ToString(),
                CreatedAt = movement.CreatedAt
            };
        }
    }
}
