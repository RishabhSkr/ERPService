using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.StockMovements;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Inventory;
using MyERP.Services.Inventory.Repositories.Products;
using MyERP.Services.Inventory.Repositories.RawMaterials;
using MyERP.Services.Inventory.Repositories.Warehouses;
using MyERP.Services.Inventory.Constants;

namespace MyERP.Services.Inventory.Services.StockMovements
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRawMaterialRepository _rawMaterialRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStorageLocationRepository _locationRepository;

        public StockMovementService(
            IStockMovementRepository stockMovementRepository,
            IProductRepository productRepository,
            IRawMaterialRepository rawMaterialRepository,
            IWarehouseRepository warehouseRepository,
            IStorageLocationRepository locationRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _productRepository = productRepository;
            _rawMaterialRepository = rawMaterialRepository;
            _warehouseRepository = warehouseRepository;
            _locationRepository = locationRepository;
        }

        public async Task<StockMovementResponseDto> RecordMovementAsync(RecordStockMovementDto dto, Guid? userId = null)
        {
            // Validate storage location exists
            var location = await _locationRepository.GetByIdAsync(dto.StorageLocationId);
            if (location == null)
                throw new NotFoundException("StorageLocation", dto.StorageLocationId);

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
                var inventory = await _productRepository.GetInventoryAsync(dto.ItemId, dto.StorageLocationId);

                if (inventory == null)
                {
                    inventory = new ProductInventory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = dto.ItemId,
                        WarehouseId = location.WarehouseId,
                        StorageLocationId = dto.StorageLocationId,
                        CurrentStock = 0,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _productRepository.AddProductInventoryAsync(inventory);
                }

                stockBefore = inventory.CurrentStock - inventory.ReservedStock;

                // Apply movement
                switch (dto.MovementType.ToUpper())
                {
                    case MovementType.IN:
                        inventory.CurrentStock += dto.Quantity;
                        break;
                    case MovementType.OUT:
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    case MovementType.RESERVE:
                        inventory.ReservedStock += dto.Quantity;
                        break;
                    case MovementType.RELEASE:
                        inventory.ReservedStock -= dto.Quantity;
                        break;
                    case MovementType.ADJUST:
                        inventory.CurrentStock = dto.Quantity;
                        break;
                    case MovementType.ADJUST_OUT:
                        if (inventory.CurrentStock < dto.Quantity)
                            throw new BadRequestException(
                                $"Insufficient stock. Need {dto.Quantity}, current {inventory.CurrentStock}");
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    default:
                        throw new BadRequestException($"Invalid movement type: {dto.MovementType}");
                }

                stockAfter = inventory.CurrentStock - inventory.ReservedStock;
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
                var inventory = await _rawMaterialRepository.GetInventoryAsync(dto.ItemId, dto.StorageLocationId);

                // Fallback: if batch didn't match, find ANY existing record for this material+location
                if (inventory == null)
                {
                    var allInventories = await _rawMaterialRepository.GetInventoriesAsync(dto.ItemId);
                    inventory = allInventories.FirstOrDefault(i => i.StorageLocationId == dto.StorageLocationId);
                }

                if (inventory == null)
                {
                    // Truly new — no record exists at all
                    inventory = new RawMaterialInventory
                    {
                        Id = Guid.NewGuid(),
                        RawMaterialId = dto.ItemId,
                        WarehouseId = location.WarehouseId,
                        StorageLocationId = dto.StorageLocationId,
                        CurrentStock = 0,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _rawMaterialRepository.AddInventoryAsync(inventory);
                }

                stockBefore = inventory.CurrentStock - inventory.ReservedStock;

                // Apply movement
                switch (dto.MovementType.ToUpper())
                {
                    case MovementType.IN:
                        inventory.CurrentStock += dto.Quantity;
                        break;
                    case MovementType.OUT:
                        if (inventory.CurrentStock < dto.Quantity)
                            throw new BadRequestException(
                                $"Insufficient stock. Need {dto.Quantity}, current {inventory.CurrentStock}");
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    case MovementType.RESERVE:
                        // Check: AvailableStock (CurrentStock - ReservedStock) >= Quantity?
                        var available = inventory.CurrentStock - inventory.ReservedStock;
                        if (available < dto.Quantity)
                            throw new BadRequestException(
                                $"Insufficient available stock. Need {dto.Quantity}, available {available}");
                        inventory.ReservedStock += dto.Quantity;
                        break;
                    case MovementType.RELEASE:
                        inventory.ReservedStock -= dto.Quantity;
                        break;
                    case MovementType.ADJUST:
                        inventory.CurrentStock = dto.Quantity;
                        break;
                    case MovementType.ADJUST_OUT:
                        if (inventory.CurrentStock < dto.Quantity)
                            throw new BadRequestException(
                                $"Insufficient stock. Need {dto.Quantity}, current {inventory.CurrentStock}");
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    case MovementType.SCRAP:
                        inventory.CurrentStock -= dto.Quantity;
                        break;
                    default:
                        throw new BadRequestException($"Invalid movement type: {dto.MovementType}");
                }

                stockAfter = inventory.CurrentStock - inventory.ReservedStock;
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
                ToWarehouseId = dto.MovementType.ToUpper() == MovementType.IN ? location.WarehouseId : null,
                ToLocationId = dto.MovementType.ToUpper() == MovementType.IN ? dto.StorageLocationId : null,
                FromWarehouseId = (dto.MovementType.ToUpper() is MovementType.OUT or MovementType.ADJUST_OUT or MovementType.RESERVE or MovementType.RELEASE or MovementType.SCRAP) ? location.WarehouseId : null,
                FromLocationId = (dto.MovementType.ToUpper() is MovementType.OUT or MovementType.ADJUST_OUT or MovementType.RESERVE or MovementType.RELEASE or MovementType.SCRAP) ? dto.StorageLocationId : null,
                MovementType = dto.MovementType.ToUpper(),
                Quantity = dto.Quantity,
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                ReferenceType = dto.ReferenceType,
                ReferenceId = dto.ReferenceId,
                WorkOrderId = dto.WorkOrderId,
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
                FromLocationCode = movement.FromLocation?.LocationCode,
                ToLocationCode = movement.ToLocation?.LocationCode,
                CreatedBy = movement.CreatedBy?.ToString(),
                CreatedAt = movement.CreatedAt
            };
        }

        public async Task<StockMovementResponseDto> TransferStockAsync(TransferStockDto dto, Guid? userId = null)
        {
            var fromLocation = await _locationRepository.GetByIdAsync(dto.FromStorageLocationId);
            if (fromLocation == null) throw new NotFoundException("StorageLocation", dto.FromStorageLocationId);

            var toLocation = await _locationRepository.GetByIdAsync(dto.ToStorageLocationId);
            if (toLocation == null) throw new NotFoundException("StorageLocation", dto.ToStorageLocationId);

            string itemCode;
            string itemName;
            decimal stockBefore;
            decimal stockAfter;

            if (dto.ItemType.ToLower() == "product")
            {
                var product = await _productRepository.GetByIdAsync(dto.ItemId);
                if (product == null) throw new NotFoundException("Product", dto.ItemId);

                if (toLocation.LocationType != null && !toLocation.LocationType.AllowProducts)
                    throw new BadRequestException($"Destination Location '{toLocation.LocationCode}' does not allow storing Finished Goods.");

                itemCode = product.ProductCode;
                itemName = product.ProductName;

                var fromInventory = await _productRepository.GetInventoryAsync(dto.ItemId, dto.FromStorageLocationId);
                if (fromInventory == null) throw new BadRequestException($"No stock found for this product in Source Location.");

                var available = fromInventory.CurrentStock - fromInventory.ReservedStock;
                if (available < dto.Quantity)
                    throw new BadRequestException($"Insufficient available stock. Need {dto.Quantity}, available {available}");

                // Add to Destination
                var toInventory = await _productRepository.GetInventoryAsync(dto.ItemId, dto.ToStorageLocationId);
                if (toInventory == null)
                {
                    toInventory = new ProductInventory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = dto.ItemId,
                        WarehouseId = toLocation.WarehouseId,
                        StorageLocationId = dto.ToStorageLocationId,
                        CurrentStock = 0,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _productRepository.AddProductInventoryAsync(toInventory);
                }

                stockBefore = fromInventory.CurrentStock;
                
                fromInventory.CurrentStock -= dto.Quantity;
                fromInventory.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateInventoryAsync(fromInventory);

                toInventory.CurrentStock += dto.Quantity;
                toInventory.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateInventoryAsync(toInventory);
                
                stockAfter = fromInventory.CurrentStock;
            }
            else if (dto.ItemType.ToLower() == "rawmaterial")
            {
                var rawMaterial = await _rawMaterialRepository.GetByIdAsync(dto.ItemId);
                if (rawMaterial == null) throw new NotFoundException("RawMaterial", dto.ItemId);

                if (toLocation.LocationType != null && !toLocation.LocationType.AllowRawMaterials)
                    throw new BadRequestException($"Destination Location '{toLocation.LocationCode}' does not allow storing Raw Materials.");

                itemCode = rawMaterial.MaterialCode;
                itemName = rawMaterial.MaterialName;

                var allInventories = await _rawMaterialRepository.GetInventoriesAsync(dto.ItemId);
                var fromInventory = allInventories.FirstOrDefault(i => i.StorageLocationId == dto.FromStorageLocationId);
                
                if (fromInventory == null) throw new BadRequestException($"No stock found for this raw material in Source Location.");

                var available = fromInventory.CurrentStock - fromInventory.ReservedStock;
                if (available < dto.Quantity)
                    throw new BadRequestException($"Insufficient available stock. Need {dto.Quantity}, available {available}");

                var toInventory = allInventories.FirstOrDefault(i => i.StorageLocationId == dto.ToStorageLocationId);
                if (toInventory == null)
                {
                    toInventory = new RawMaterialInventory
                    {
                        Id = Guid.NewGuid(),
                        RawMaterialId = dto.ItemId,
                        WarehouseId = toLocation.WarehouseId,
                        StorageLocationId = dto.ToStorageLocationId,
                        CurrentStock = 0,
                        ReservedStock = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _rawMaterialRepository.AddInventoryAsync(toInventory);
                }

                stockBefore = fromInventory.CurrentStock;
                
                fromInventory.CurrentStock -= dto.Quantity;
                fromInventory.UpdatedAt = DateTime.UtcNow;
                await _rawMaterialRepository.UpdateInventoryAsync(fromInventory);

                toInventory.CurrentStock += dto.Quantity;
                toInventory.UpdatedAt = DateTime.UtcNow;
                await _rawMaterialRepository.UpdateInventoryAsync(toInventory);
                
                stockAfter = fromInventory.CurrentStock;
            }
            else
            {
                throw new BadRequestException($"Invalid item type: {dto.ItemType}. Must be 'Product' or 'RawMaterial'");
            }

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                ItemType = dto.ItemType,
                ItemId = dto.ItemId,
                ItemCode = itemCode,
                ItemName = itemName,
                FromWarehouseId = fromLocation.WarehouseId,
                FromLocationId = dto.FromStorageLocationId,
                ToWarehouseId = toLocation.WarehouseId,
                ToLocationId = dto.ToStorageLocationId,
                MovementType = MovementType.TRANSFER,
                Quantity = dto.Quantity,
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                Notes = dto.Notes,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _stockMovementRepository.AddAsync(movement);

            return MapToDto(movement);
        }
    }
}
