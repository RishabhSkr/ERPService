using MyERP.Services.Inventory.DTOs.Warehouse;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Warehouses;

namespace MyERP.Services.Inventory.Services.Warehouses
{
    public class StorageLocationService : IStorageLocationService
    {
        private readonly IStorageLocationRepository _storageLocationRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public StorageLocationService(
            IStorageLocationRepository storageLocationRepository,
            IWarehouseRepository warehouseRepository)
        {
            _storageLocationRepository = storageLocationRepository;
            _warehouseRepository = warehouseRepository;
        }

        public async Task<StorageLocationDto> GetByIdAsync(Guid id)
        {
            var location = await _storageLocationRepository.GetByIdAsync(id);
            if (location == null)
                throw new NotFoundException("StorageLocation", id);

            return MapToDto(location);
        }

        public async Task<IEnumerable<StorageLocationDto>> GetByWarehouseIdAsync(Guid warehouseId)
        {
            var exists = await _warehouseRepository.ExistsAsync(warehouseId);
            if (!exists)
                throw new NotFoundException("Warehouse", warehouseId);

            var locations = await _storageLocationRepository.GetByWarehouseIdAsync(warehouseId);
            return locations.Select(MapToDto);
        }

        public async Task<IEnumerable<StorageLocationDto>> GetAllAsync()
        {
            var locations = await _storageLocationRepository.GetAllAsync();
            return locations.Select(MapToDto);
        }

        public async Task<StorageLocationDto> CreateAsync(CreateStorageLocationDto dto)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new NotFoundException("Warehouse", dto.WarehouseId);

            var locationCode = GenerateLocationCode(warehouse.WarehouseCode, dto.Zone, dto.Aisle, dto.Rack, dto.Level, dto.Bin);

            if (await _storageLocationRepository.LocationCodeExistsAsync(locationCode, dto.WarehouseId))
                throw new ConflictException($"A storage location with code {locationCode} already exists in this warehouse.");

            var location = new StorageLocation
            {
                Id = Guid.NewGuid(),
                WarehouseId = dto.WarehouseId,
                Zone = dto.Zone,
                Aisle = dto.Aisle,
                Rack = dto.Rack,
                Level = dto.Level,
                Bin = dto.Bin,
                LocationCode = locationCode,
                LocationTypeId = dto.LocationTypeId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _storageLocationRepository.AddAsync(location);
            // Fetch with Warehouse for DTO mapping
            var created = await _storageLocationRepository.GetByIdAsync(result.Id);
            
            return MapToDto(created!);
        }

        public async Task<StorageLocationDto> UpdateAsync(Guid id, UpdateStorageLocationDto dto)
        {
            var location = await _storageLocationRepository.GetByIdAsync(id);
            if (location == null)
                throw new NotFoundException("StorageLocation", id);

            if (dto.WarehouseId.HasValue && dto.WarehouseId.Value != location.WarehouseId)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId.Value);
                if (warehouse == null)
                    throw new NotFoundException("Warehouse", dto.WarehouseId.Value);
                
                location.WarehouseId = dto.WarehouseId.Value;
                location.Warehouse = warehouse;
            }

            var newLocationCode = GenerateLocationCode(location.Warehouse!.WarehouseCode, dto.Zone, dto.Aisle, dto.Rack, dto.Level, dto.Bin);

            if (await _storageLocationRepository.LocationCodeExistsAsync(newLocationCode, location.WarehouseId, id))
                throw new ConflictException($"A storage location with code {newLocationCode} already exists in this warehouse.");

            location.Zone = dto.Zone;
            location.Aisle = dto.Aisle;
            location.Rack = dto.Rack;
            location.Level = dto.Level;
            location.Bin = dto.Bin;
            location.LocationCode = newLocationCode;
            location.LocationTypeId = dto.LocationTypeId;
            location.IsActive = dto.IsActive;
            location.UpdatedAt = DateTime.UtcNow;

            await _storageLocationRepository.UpdateAsync(location);
            return MapToDto(location);
        }

        public async Task DeleteAsync(Guid id)
        {
            if (!await _storageLocationRepository.ExistsAsync(id))
                throw new NotFoundException("StorageLocation", id);

            // In a real system, you'd check if any inventory is currently in this location before deleting!
            await _storageLocationRepository.DeleteAsync(id);
        }

        private static string GenerateLocationCode(string warehouseCode, string? zone, string? aisle, string? rack, string? level, string? bin)
        {
            var parts = new List<string> { warehouseCode };
            if (!string.IsNullOrWhiteSpace(zone)) parts.Add($"Z{zone}");
            if (!string.IsNullOrWhiteSpace(aisle)) parts.Add($"A{aisle}");
            if (!string.IsNullOrWhiteSpace(rack)) parts.Add($"R{rack}");
            if (!string.IsNullOrWhiteSpace(level)) parts.Add($"L{level}");
            if (!string.IsNullOrWhiteSpace(bin)) parts.Add($"B{bin}");
            
            return string.Join("-", parts).ToUpperInvariant();
        }

        private static StorageLocationDto MapToDto(StorageLocation location)
        {
            return new StorageLocationDto
            {
                Id = location.Id,
                WarehouseId = location.WarehouseId,
                WarehouseName = location.Warehouse?.WarehouseName ?? string.Empty,
                Zone = location.Zone,
                Aisle = location.Aisle,
                Rack = location.Rack,
                Level = location.Level,
                Bin = location.Bin,
                LocationCode = location.LocationCode,
                LocationTypeId = location.LocationTypeId,
                LocationTypeName = location.LocationType?.TypeName ?? string.Empty,
                AllowRawMaterials = location.LocationType?.AllowRawMaterials ?? true,
                AllowProducts = location.LocationType?.AllowProducts ?? true,
                IsActive = location.IsActive,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            };
        }
    }
}
