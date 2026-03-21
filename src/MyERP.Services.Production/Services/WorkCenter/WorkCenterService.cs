using MyERP.Services.Production.DTOs.WorkCenter;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Repositories.WorkCenter;

namespace MyERP.Services.Production.Services.WorkCenter
{
    public class WorkCenterService : IWorkCenterService
    {
        private readonly IWorkCenterRepository _repository;
        private readonly ILogger<WorkCenterService> _logger;

        public WorkCenterService(IWorkCenterRepository repository, ILogger<WorkCenterService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<WorkCenterDto> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdWithEquipmentAsync(id);
            if (entity == null) throw new NotFoundException("WorkCenter", id);
            return MapToDto(entity);
        }

        public async Task<IEnumerable<WorkCenterDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<WorkCenterDto> CreateAsync(CreateWorkCenterDto dto)
        {
            if (await _repository.ExistsByCodeAsync(dto.CenterCode))
                throw new AppException($"WorkCenter with code '{dto.CenterCode}' already exists");

            var entity = new Models.WorkCenter
            {
                WorkCenterId = Guid.NewGuid(),
                CenterCode = dto.CenterCode,
                CenterName = dto.CenterName,
                Location = dto.Location,
                CostPerHour = dto.CostPerHour,
                CapacityPerHour = dto.CapacityPerHour,
                Description = dto.Description
            };

            await _repository.CreateAsync(entity);
            _logger.LogInformation("WorkCenter created: {Code}", dto.CenterCode);
            return MapToDto(entity);
        }

        public async Task<WorkCenterDto> UpdateAsync(Guid id, CreateWorkCenterDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("WorkCenter", id);

            entity.CenterName = dto.CenterName;
            entity.Location = dto.Location;
            entity.CostPerHour = dto.CostPerHour;
            entity.CapacityPerHour = dto.CapacityPerHour;
            entity.Description = dto.Description;

            await _repository.UpdateAsync(entity);
            return MapToDto(entity);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("WorkCenter", id);
            await _repository.DeleteAsync(id);
        }

        private static WorkCenterDto MapToDto(Models.WorkCenter wc) => new()
        {
            WorkCenterId = wc.WorkCenterId, CenterCode = wc.CenterCode,
            CenterName = wc.CenterName, Location = wc.Location,
            CostPerHour = wc.CostPerHour, CapacityPerHour = wc.CapacityPerHour,
            Description = wc.Description, IsActive = wc.IsActive,
            EquipmentCount = wc.Equipment?.Count ?? 0
        };
    }
}
