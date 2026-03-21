using MyERP.Services.Production.DTOs.Equipment;
using MyERP.Services.Production.DTOs.Process;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Repositories.Equipment;

namespace MyERP.Services.Production.Services.Equipment
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _repository;
        private readonly ILogger<EquipmentService> _logger;

        public EquipmentService(IEquipmentRepository repository, ILogger<EquipmentService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<EquipmentDto> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdWithWorkCenterAsync(id);
            if (entity == null) throw new NotFoundException("Equipment", id);

            var dto = MapToDto(entity);
            var links = await _repository.GetLinkedProcessesAsync(id);
            dto.LinkedProcesses = links.Where(ep => ep.Process != null).Select(ep => new ProcessDto
            {
                ProcessId = ep.Process!.ProcessId, ProcessCode = ep.Process.ProcessCode,
                ProcessName = ep.Process.ProcessName, Category = ep.Process.Category,
                StandardTimeMinutes = ep.Process.StandardTimeMinutes,
                Description = ep.Process.Description, IsActive = ep.Process.IsActive
            }).ToList();
            return dto;
        }

        public async Task<IEnumerable<EquipmentDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<IEnumerable<EquipmentDto>> GetByWorkCenterAsync(Guid workCenterId)
        {
            var entities = await _repository.GetByWorkCenterAsync(workCenterId);
            return entities.Select(MapToDto);
        }

        public async Task<EquipmentDto> CreateAsync(CreateEquipmentDto dto)
        {
            if (await _repository.ExistsByCodeAsync(dto.EquipmentCode))
                throw new AppException($"Equipment with code '{dto.EquipmentCode}' already exists");

            var entity = new Models.Equipment
            {
                EquipmentId = Guid.NewGuid(),
                EquipmentCode = dto.EquipmentCode,
                EquipmentName = dto.EquipmentName,
                WorkCenterId = dto.WorkCenterId,
                Manufacturer = dto.Manufacturer,
                Model = dto.Model,
                CostPerHour = dto.CostPerHour
            };

            await _repository.CreateAsync(entity);
            _logger.LogInformation("Equipment created: {Code}", dto.EquipmentCode);
            return await GetByIdAsync(entity.EquipmentId);
        }

        public async Task<EquipmentDto> UpdateAsync(Guid id, CreateEquipmentDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("Equipment", id);

            entity.EquipmentName = dto.EquipmentName;
            entity.Manufacturer = dto.Manufacturer;
            entity.Model = dto.Model;
            entity.CostPerHour = dto.CostPerHour;

            await _repository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("Equipment", id);
            await _repository.DeleteAsync(id);
        }

        public async Task LinkProcessesAsync(Guid equipmentId, LinkProcessesDto dto)
        {
            var entity = await _repository.GetByIdAsync(equipmentId);
            if (entity == null) throw new NotFoundException("Equipment", equipmentId);
            await _repository.ReplaceLinkedProcessesAsync(equipmentId, dto.ProcessIds);
            _logger.LogInformation("Equipment {Code}: linked {Count} processes", entity.EquipmentCode, dto.ProcessIds.Count);
        }

        public async Task<IEnumerable<ProcessDto>> GetLinkedProcessesAsync(Guid equipmentId)
        {
            var links = await _repository.GetLinkedProcessesAsync(equipmentId);
            return links.Where(ep => ep.Process != null).Select(ep => new ProcessDto
            {
                ProcessId = ep.Process!.ProcessId, ProcessCode = ep.Process.ProcessCode,
                ProcessName = ep.Process.ProcessName, Category = ep.Process.Category,
                StandardTimeMinutes = ep.Process.StandardTimeMinutes,
                Description = ep.Process.Description, IsActive = ep.Process.IsActive
            });
        }

        private static EquipmentDto MapToDto(Models.Equipment e) => new()
        {
            EquipmentId = e.EquipmentId, EquipmentCode = e.EquipmentCode,
            EquipmentName = e.EquipmentName, WorkCenterId = e.WorkCenterId,
            WorkCenterCode = e.WorkCenter?.CenterCode ?? "",
            WorkCenterName = e.WorkCenter?.CenterName ?? "",
            Manufacturer = e.Manufacturer, Model = e.Model,
            Status = e.Status, CostPerHour = e.CostPerHour, IsActive = e.IsActive
        };
    }
}
