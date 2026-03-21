using MyERP.Services.Inventory.DTOs.Units;
using MyERP.Services.Inventory.Exceptions;
using MyERP.Services.Inventory.Models;
using MyERP.Services.Inventory.Repositories.Units;

namespace MyERP.Services.Inventory.Services.Units
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _unitRepository;

        public UnitService(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<UnitResponseDto> CreateAsync(CreateUnitDto dto)
        {
            var exists = await _unitRepository.CodeExistsAsync(dto.UnitCode);
            if (exists)
                throw new ConflictException($"Unit with code '{dto.UnitCode}' already exists");

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                UnitName = dto.UnitName,
                UnitCode = dto.UnitCode.ToUpper(),
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitRepository.AddAsync(unit);

            return MapToDto(unit);
        }

        public async Task<List<UnitResponseDto>> GetAllAsync()
        {
            var units = await _unitRepository.GetAllActiveAsync();
            return units.Select(MapToDto).ToList();
        }

        public async Task<UnitResponseDto> GetByIdAsync(Guid id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
                throw new NotFoundException("Unit", id);

            return MapToDto(unit);
        }

        public async Task<UnitResponseDto> UpdateAsync(Guid id, CreateUnitDto dto)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
                throw new NotFoundException("Unit", id);

            var codeExists = await _unitRepository.CodeExistsAsync(dto.UnitCode, id);
            if (codeExists)
                throw new ConflictException($"Unit with code '{dto.UnitCode}' already exists");

            unit.UnitName = dto.UnitName;
            unit.UnitCode = dto.UnitCode.ToUpper();
            unit.Description = dto.Description;
            unit.UpdatedAt = DateTime.UtcNow;

            await _unitRepository.UpdateAsync(unit);

            return MapToDto(unit);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
                throw new NotFoundException("Unit", id);

            var isInUse = await _unitRepository.IsInUseAsync(id);
            if (isInUse)
                throw new ConflictException("Cannot delete unit. It is being used by products or raw materials");

            unit.IsActive = false;
            unit.UpdatedAt = DateTime.UtcNow;
            await _unitRepository.UpdateAsync(unit);

            return true;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
                throw new NotFoundException("Unit", id);

            if (unit.IsActive)
                throw new BadRequestException("Unit is already active");

            unit.IsActive = true;
            unit.UpdatedAt = DateTime.UtcNow;
            await _unitRepository.UpdateAsync(unit);

            return true;
        }

        private static UnitResponseDto MapToDto(Unit unit)
        {
            return new UnitResponseDto
            {
                Id = unit.Id,
                UnitName = unit.UnitName,
                UnitCode = unit.UnitCode,
                Description = unit.Description,
                IsActive = unit.IsActive,
                CreatedAt = unit.CreatedAt
            };
        }
    }
}
