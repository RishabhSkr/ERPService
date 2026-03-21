using MyERP.Services.Production.DTOs.Process;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Repositories.Process;

namespace MyERP.Services.Production.Services.Process
{
    public class ProcessService : IProcessService
    {
        private readonly IProcessRepository _repository;
        private readonly ILogger<ProcessService> _logger;

        public ProcessService(IProcessRepository repository, ILogger<ProcessService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProcessDto> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("Process", id);
            return MapToDto(entity);
        }

        public async Task<IEnumerable<ProcessDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<ProcessDto> CreateAsync(CreateProcessDto dto)
        {
            if (await _repository.ExistsByCodeAsync(dto.ProcessCode))
                throw new AppException($"Process with code '{dto.ProcessCode}' already exists");

            var entity = new Models.Process
            {
                ProcessId = Guid.NewGuid(),
                ProcessCode = dto.ProcessCode,
                ProcessName = dto.ProcessName,
                Category = dto.Category,
                StandardTimeMinutes = dto.StandardTimeMinutes,
                Description = dto.Description
            };

            await _repository.CreateAsync(entity);
            _logger.LogInformation("Process created: {Code}", dto.ProcessCode);
            return MapToDto(entity);
        }

        public async Task<ProcessDto> UpdateAsync(Guid id, CreateProcessDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("Process", id);

            entity.ProcessName = dto.ProcessName;
            entity.Category = dto.Category;
            entity.StandardTimeMinutes = dto.StandardTimeMinutes;
            entity.Description = dto.Description;

            await _repository.UpdateAsync(entity);
            return MapToDto(entity);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("Process", id);
            await _repository.DeleteAsync(id);
            _logger.LogInformation("Process deactivated: {Id}", id);
        }

        private static ProcessDto MapToDto(Models.Process p) => new()
        {
            ProcessId = p.ProcessId, ProcessCode = p.ProcessCode,
            ProcessName = p.ProcessName, Category = p.Category,
            StandardTimeMinutes = p.StandardTimeMinutes,
            Description = p.Description, IsActive = p.IsActive
        };
    }
}
