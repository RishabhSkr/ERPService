using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Warehouses;

namespace MyERP.Services.Inventory.Services.Warehouses
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return warehouses.Select(w => new WarehouseDto
            {
                Id = w.Id,
                WarehouseName = w.WarehouseName,
                WarehouseCode = w.WarehouseCode,
                Address = w.Address,
                City = w.City,
                IsActive = w.IsActive
            });
        }

        public async Task<WarehouseDto> GetByIdAsync(Guid id)
        {
            var w = await _warehouseRepository.GetByIdWithDetailsAsync(id);
            if (w == null)
                throw new NotFoundException("Warehouse", id);

            var dto = new WarehouseDto
            {
                Id = w.Id,
                WarehouseName = w.WarehouseName,
                WarehouseCode = w.WarehouseCode,
                Address = w.Address,
                City = w.City,
                IsActive = w.IsActive,
                StorageLocations = new List<StorageLocationDetailDto>()
            };

            if (w.StorageLocations != null)
            {
                foreach (var sl in w.StorageLocations)
                {
                    var slDto = new StorageLocationDetailDto
                    {
                        Id = sl.Id,
                        LocationCode = sl.LocationCode,
                        LocationTypeName = sl.LocationType?.TypeName ?? "Unknown",
                        Zone = sl.Zone,
                        Rack = sl.Rack,
                        IsActive = sl.IsActive,
                        StoredItems = new List<StoredItemDto>()
                    };

                    if (sl.ProductInventories != null)
                    {
                        foreach (var pi in sl.ProductInventories.Where(p => p.CurrentStock > 0))
                        {
                            slDto.StoredItems.Add(new StoredItemDto
                            {
                                ItemType = "Product",
                                ItemCode = pi.Product?.ProductCode ?? "",
                                ItemName = pi.Product?.ProductName ?? "",
                                CurrentStock = pi.CurrentStock,
                                ReservedStock = pi.ReservedStock,
                                AvailableStock = pi.CurrentStock - pi.ReservedStock
                            });
                        }
                    }

                    if (sl.RawMaterialInventories != null)
                    {
                        foreach (var ri in sl.RawMaterialInventories.Where(r => r.CurrentStock > 0))
                        {
                            slDto.StoredItems.Add(new StoredItemDto
                            {
                                ItemType = "RawMaterial",
                                ItemCode = ri.RawMaterial?.MaterialCode ?? "",
                                ItemName = ri.RawMaterial?.MaterialName ?? "",
                                CurrentStock = ri.CurrentStock,
                                ReservedStock = ri.ReservedStock,
                                AvailableStock = ri.CurrentStock - ri.ReservedStock
                            });
                        }
                    }

                    dto.StorageLocations.Add(slDto);
                }
            }

            return dto;
        }

        public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
        {
            var warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                WarehouseName = dto.WarehouseName,
                WarehouseCode = dto.WarehouseCode,
                Address = dto.Address,
                City = dto.City,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _warehouseRepository.AddAsync(warehouse);

            return new WarehouseDto
            {
                Id = warehouse.Id,
                WarehouseName = warehouse.WarehouseName,
                WarehouseCode = warehouse.WarehouseCode,
                Address = warehouse.Address,
                City = warehouse.City,
                IsActive = warehouse.IsActive
            };
        }

        public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto dto)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new NotFoundException("Warehouse", id);

            warehouse.WarehouseName = dto.WarehouseName;
            warehouse.Address = dto.Address;
            warehouse.City = dto.City;
            warehouse.IsActive = dto.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.UpdateAsync(warehouse);

            return new WarehouseDto
            {
                Id = warehouse.Id,
                WarehouseName = warehouse.WarehouseName,
                WarehouseCode = warehouse.WarehouseCode,
                Address = warehouse.Address,
                City = warehouse.City,
                IsActive = warehouse.IsActive
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
                throw new NotFoundException("Warehouse", id);

            await _warehouseRepository.DeleteAsync(id);
        }
    }
}
