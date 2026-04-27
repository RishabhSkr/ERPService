using MyERP.Services.Inventory.DTOs.Warehouse;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Warehouses;

namespace MyERP.Services.Inventory.Services.Warehouses
{
    public class StorageLocationTypeService : IStorageLocationTypeService
    {
        private readonly IStorageLocationTypeRepository _repository;

        public StorageLocationTypeService(IStorageLocationTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<StorageLocationTypeDto>> GetAllAsync()
        {
            var types = await _repository.GetAllAsync();
            return types.Select(MapToDto);
        }

        public async Task<StorageLocationTypeDto> GetByIdAsync(Guid id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null)
                throw new NotFoundException("StorageLocationType", id);

            return MapToDto(type);
        }

        public async Task<StorageLocationTypeDto> CreateAsync(CreateStorageLocationTypeDto dto)
        {
            if (await _repository.TypeCodeExistsAsync(dto.TypeCode))
                throw new ConflictException($"Storage Location Type code '{dto.TypeCode}' already exists.");

            var type = new StorageLocationType
            {
                Id = Guid.NewGuid(),
                TypeCode = dto.TypeCode,
                TypeName = dto.TypeName,
                AllowRawMaterials = dto.AllowRawMaterials,
                AllowProducts = dto.AllowProducts,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(type);
            return MapToDto(type);
        }

        public async Task<StorageLocationTypeDto> UpdateAsync(Guid id, UpdateStorageLocationTypeDto dto)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null)
                throw new NotFoundException("StorageLocationType", id);

            type.TypeName = dto.TypeName;
            type.AllowRawMaterials = dto.AllowRawMaterials;
            type.AllowProducts = dto.AllowProducts;
            type.IsActive = dto.IsActive;
            type.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(type);
            return MapToDto(type);
        }

        public async Task DeleteAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
                throw new NotFoundException("StorageLocationType", id);

            // Need to check if any StorageLocation is currently using this type!
            // But for now, we just delete or let DB constraint handle it.
            await _repository.DeleteAsync(id);
        }

        private static StorageLocationTypeDto MapToDto(StorageLocationType type)
        {
            return new StorageLocationTypeDto
            {
                Id = type.Id,
                TypeCode = type.TypeCode,
                TypeName = type.TypeName,
                AllowRawMaterials = type.AllowRawMaterials,
                AllowProducts = type.AllowProducts,
                IsActive = type.IsActive,
                CreatedAt = type.CreatedAt,
                UpdatedAt = type.UpdatedAt
            };
        }
    }
}
