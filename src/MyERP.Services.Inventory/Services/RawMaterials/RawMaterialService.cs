using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.DTOs.Products;
using MyERP.Services.Inventory.DTOs.RawMaterials;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Categories;
using MyERP.Services.Inventory.Repositories.RawMaterials;
using MyERP.Services.Inventory.Repositories.Units;
using MyERP.Services.Inventory.Repositories.Warehouses;

namespace MyERP.Services.Inventory.Services.RawMaterials
{
    public class RawMaterialService : IRawMaterialService
    {
        private readonly IRawMaterialRepository _rawMaterialRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStorageLocationRepository _locationRepository;

        public RawMaterialService(
            IRawMaterialRepository rawMaterialRepository,
            ICategoryRepository categoryRepository,
            IUnitRepository unitRepository,
            IWarehouseRepository warehouseRepository,
            IStorageLocationRepository locationRepository)
        {
            _rawMaterialRepository = rawMaterialRepository;
            _categoryRepository = categoryRepository;
            _unitRepository = unitRepository;
            _warehouseRepository = warehouseRepository;
            _locationRepository = locationRepository;
        }

        public async Task<RawMaterialResponseDto> CreateAsync(CreateRawMaterialDto dto)
        {
            var exists = await _rawMaterialRepository.CodeExistsAsync(dto.MaterialCode);
            if (exists)
                throw new ConflictException($"Raw material with code '{dto.MaterialCode}' already exists");

            var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
                throw new NotFoundException("Category", dto.CategoryId);

            var unitExists = await _unitRepository.ExistsAsync(dto.UnitId);
            if (!unitExists)
                throw new NotFoundException("Unit", dto.UnitId);

            if (dto.DefaultStorageLocationId.HasValue)
            {
                var loc = await _locationRepository.GetByIdAsync(dto.DefaultStorageLocationId.Value);
                if (loc == null)
                    throw new NotFoundException("StorageLocation", dto.DefaultStorageLocationId.Value);
                if (loc.LocationType != null && !loc.LocationType.AllowRawMaterials)
                    throw new BadRequestException($"Storage Location '{loc.LocationCode}' does not allow storing Raw Materials. Its type is '{loc.LocationType.TypeName}'.");
            }

            var rawMaterial = new RawMaterial
            {
                Id = Guid.NewGuid(),
                MaterialCode = dto.MaterialCode.ToUpper(),
                MaterialName = dto.MaterialName,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                UnitId = dto.UnitId,
                Cost = dto.Cost,
                MinStockLevel = dto.MinStockLevel,
                Supplier = dto.Supplier,
                DefaultStorageLocationId = dto.DefaultStorageLocationId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _rawMaterialRepository.AddAsync(rawMaterial);

            return await GetByIdAsync(rawMaterial.Id);
        }

        public async Task<PagedResponse<RawMaterialListDto>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? categoryId = null,
            string? searchKeyword = null)
        {
            var (materials, totalCount) = await _rawMaterialRepository.GetAllAsync(
                pageNumber, pageSize, categoryId, searchKeyword);

            var data = materials.Select(r => new RawMaterialListDto
            {
                Id = r.Id,
                MaterialCode = r.MaterialCode,
                MaterialName = r.MaterialName,
                CategoryName = r.Category?.CategoryName ?? "",
                Cost = r.Cost,
                CurrentStock = r.RawMaterialInventories?.Sum(i => i.CurrentStock) ?? 0,
                ReservedStock = r.RawMaterialInventories?.Sum(i => i.ReservedStock) ?? 0,
                AvailableStock = r.RawMaterialInventories?.Sum(i => i.CurrentStock - i.ReservedStock) ?? 0,
                UnitName = r.Unit?.UnitName ?? "",
                Supplier = r.Supplier,
                MinStockLevel = r.MinStockLevel,
                DefaultStorageLocationId = r.DefaultStorageLocationId,
                DefaultStorageLocationCode = r.DefaultStorageLocation?.LocationCode,
                IsActive = r.IsActive,
                LocationStocks = r.RawMaterialInventories?
                    .Where(i => i.CurrentStock > 0 || i.ReservedStock > 0)
                    .Select(i => new LocationStockDto
                    {
                        StorageLocationId = i.StorageLocationId,
                        LocationCode = i.StorageLocation?.LocationCode ?? "",
                        WarehouseName = i.Warehouse?.WarehouseName ?? "",
                        CurrentStock = i.CurrentStock,
                        ReservedStock = i.ReservedStock,
                        AvailableStock = i.AvailableStock
                    }).ToList() ?? new()
            }).ToList();

            return new PagedResponse<RawMaterialListDto>(data, pageNumber, pageSize, totalCount);
        }

        public async Task<RawMaterialResponseDto> GetByIdAsync(Guid id)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdWithInventoryAsync(id);
            if (rawMaterial == null)
                throw new NotFoundException("RawMaterial", id);

            return MapToDto(rawMaterial);
        }

        public async Task<RawMaterialResponseDto> UpdateAsync(Guid id, UpdateRawMaterialDto dto)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(id);
            if (rawMaterial == null)
                throw new NotFoundException("RawMaterial", id);

            if (!string.IsNullOrWhiteSpace(dto.MaterialName))
                rawMaterial.MaterialName = dto.MaterialName;

            if (dto.Description != null)
                rawMaterial.Description = dto.Description;

            if (dto.Cost.HasValue)
                rawMaterial.Cost = dto.Cost.Value;

            if (dto.MinStockLevel.HasValue)
                rawMaterial.MinStockLevel = dto.MinStockLevel.Value;

            if (dto.Supplier != null)
                rawMaterial.Supplier = dto.Supplier;

            if (dto.DefaultStorageLocationId.HasValue)
            {
                var loc = await _locationRepository.GetByIdAsync(dto.DefaultStorageLocationId.Value);
                if (loc == null)
                    throw new NotFoundException("StorageLocation", dto.DefaultStorageLocationId.Value);
                if (loc.LocationType != null && !loc.LocationType.AllowRawMaterials)
                    throw new BadRequestException($"Storage Location '{loc.LocationCode}' does not allow storing Raw Materials. Its type is '{loc.LocationType.TypeName}'.");

                rawMaterial.DefaultStorageLocationId = dto.DefaultStorageLocationId.Value;
            }
            else
            {
                rawMaterial.DefaultStorageLocationId = null;
            }

            if (dto.IsActive.HasValue)
                rawMaterial.IsActive = dto.IsActive.Value;

            rawMaterial.UpdatedAt = DateTime.UtcNow;
            await _rawMaterialRepository.UpdateAsync(rawMaterial);

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(id);
            if (rawMaterial == null)
                throw new NotFoundException("RawMaterial", id);

            rawMaterial.IsActive = false;
            rawMaterial.UpdatedAt = DateTime.UtcNow;
            await _rawMaterialRepository.UpdateAsync(rawMaterial);

            return true;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(id);
            if (rawMaterial == null)
                throw new NotFoundException("RawMaterial", id);

            if (rawMaterial.IsActive)
                throw new BadRequestException("Raw material is already active");

            rawMaterial.IsActive = true;
            rawMaterial.UpdatedAt = DateTime.UtcNow;
            await _rawMaterialRepository.UpdateAsync(rawMaterial);

            return true;
        }

        public async Task<ReservationResponseDto> ReserveMaterialsAsync(ReserveRawMaterialsDto dto)
        {
            var insufficientMaterials = new List<InsufficientMaterialInfo>();
            var reservations = new List<ReservedMaterialInfo>();

            // Get default warehouse
            var warehouse = await _warehouseRepository.GetFirstActiveAsync();
            if (warehouse == null)
                throw new BadRequestException("No active warehouse found");

            foreach (var item in dto.Materials)
            {
                var rawMaterial = await _rawMaterialRepository.GetByIdWithInventoryAsync(item.RawMaterialId);
                if (rawMaterial == null)
                    throw new NotFoundException("RawMaterial", item.RawMaterialId);

                var availableStock = rawMaterial.RawMaterialInventories?.Sum(i => i.CurrentStock - i.ReservedStock) ?? 0;

                if (availableStock < item.Quantity)
                {
                    insufficientMaterials.Add(new InsufficientMaterialInfo
                    {
                        RawMaterialId = rawMaterial.Id,
                        MaterialName = rawMaterial.MaterialName,
                        RequestedQuantity = item.Quantity,
                        AvailableStock = availableStock
                    });
                }
            }

            if (insufficientMaterials.Any())
            {
                throw new InsufficientStockException("Insufficient stock for reservation", insufficientMaterials);
            }

            // Reserve all materials
            foreach (var item in dto.Materials)
            {
                var inventory = await _rawMaterialRepository.GetInventoryAsync(item.RawMaterialId, warehouse.Id);
                if (inventory == null)
                {
                    throw new BadRequestException($"Inventory record not found for material {item.RawMaterialId}");
                }

                inventory.ReservedStock += item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                reservations.Add(new ReservedMaterialInfo
                {
                    RawMaterialId = inventory.RawMaterialId,
                    MaterialName = inventory.RawMaterial?.MaterialName ?? "",
                    ReservedQuantity = item.Quantity,
                    AvailableStock = inventory.CurrentStock - inventory.ReservedStock
                });
            }

            await _rawMaterialRepository.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Message = "Materials reserved successfully",
                ProductionOrderId = dto.ProductionOrderId,
                Reservations = reservations
            };
        }

        public async Task<ReservationResponseDto> ReleaseMaterialsAsync(ReleaseRawMaterialsDto dto)
        {
            var warehouse = await _warehouseRepository.GetFirstActiveAsync();
            if (warehouse == null)
                throw new BadRequestException("No active warehouse found");

            var releasedMaterials = new List<ReservedMaterialInfo>();

            foreach (var item in dto.Materials)
            {
                var inventory = await _rawMaterialRepository.GetInventoryAsync(item.RawMaterialId, warehouse.Id);
                if (inventory == null)
                    continue;

                var releaseQty = Math.Min(item.Quantity, inventory.ReservedStock);
                inventory.ReservedStock -= releaseQty;
                inventory.UpdatedAt = DateTime.UtcNow;

                releasedMaterials.Add(new ReservedMaterialInfo
                {
                    RawMaterialId = inventory.RawMaterialId,
                    MaterialName = inventory.RawMaterial?.MaterialName ?? "",
                    ReservedQuantity = releaseQty,
                    AvailableStock = inventory.CurrentStock - inventory.ReservedStock
                });
            }

            await _rawMaterialRepository.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Message = "Materials released successfully",
                ProductionOrderId = dto.ProductionOrderId,
                Reservations = releasedMaterials
            };
        }

        public async Task<bool> AddStockAsync(Guid rawMaterialId, Guid storageLocationId, decimal quantity, string? batchNumber = null)
        {
            var rawMaterialExists = await _rawMaterialRepository.ExistsAsync(rawMaterialId);
            if (!rawMaterialExists)
                throw new NotFoundException("RawMaterial", rawMaterialId);

            var location = await _locationRepository.GetByIdAsync(storageLocationId);
            if (location == null)
                throw new NotFoundException("StorageLocation", storageLocationId);

            var inventory = await _rawMaterialRepository.GetInventoryAsync(rawMaterialId, storageLocationId, batchNumber);

            if (inventory == null)
            {
                inventory = new RawMaterialInventory
                {
                    Id = Guid.NewGuid(),
                    RawMaterialId = rawMaterialId,
                    WarehouseId = location.WarehouseId,
                    StorageLocationId = storageLocationId,
                    CurrentStock = 0,
                    ReservedStock = 0,
                    BatchNumber = batchNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _rawMaterialRepository.AddInventoryAsync(inventory);
            }

            inventory.CurrentStock += quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            await _rawMaterialRepository.UpdateInventoryAsync(inventory);

            return true;
        }

        private static RawMaterialResponseDto MapToDto(RawMaterial rawMaterial)
        {
            var inventories = rawMaterial.RawMaterialInventories ?? new List<RawMaterialInventory>();

            return new RawMaterialResponseDto
            {
                Id = rawMaterial.Id,
                MaterialCode = rawMaterial.MaterialCode,
                MaterialName = rawMaterial.MaterialName,
                Description = rawMaterial.Description,
                Category = rawMaterial.Category != null ? new CategoryInfo
                {
                    Id = rawMaterial.Category.Id,
                    CategoryName = rawMaterial.Category.CategoryName,
                    CategoryCode = rawMaterial.Category.CategoryCode
                } : null,
                Unit = rawMaterial.Unit != null ? new UnitInfo
                {
                    Id = rawMaterial.Unit.Id,
                    UnitName = rawMaterial.Unit.UnitName,
                    UnitCode = rawMaterial.Unit.UnitCode
                } : null,
                Cost = rawMaterial.Cost,
                MinStockLevel = rawMaterial.MinStockLevel,
                Supplier = rawMaterial.Supplier,
                TotalStock = inventories.Sum(i => i.CurrentStock),
                TotalReserved = inventories.Sum(i => i.ReservedStock),
                TotalAvailable = inventories.Sum(i => i.CurrentStock - i.ReservedStock),
                IsActive = rawMaterial.IsActive,
                CreatedAt = rawMaterial.CreatedAt,
                UpdatedAt = rawMaterial.UpdatedAt,
                WarehouseStock = inventories.Select(i => new WarehouseStockInfo
                {
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse?.WarehouseName ?? "",
                    CurrentStock = i.CurrentStock,
                    ReservedStock = i.ReservedStock,
                    AvailableStock = i.CurrentStock - i.ReservedStock
                }).ToList()
            };
        }
    }
}
