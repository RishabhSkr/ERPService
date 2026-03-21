using MyERP.Services.Production.DTOs.ProcessRoute;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.ProcessRoute;

namespace MyERP.Services.Production.Services.ProcessRoute
{
    public class ProcessRouteService : IProcessRouteService
    {
        private readonly IProcessRouteRepository _repository;
        private readonly ILogger<ProcessRouteService> _logger;

        public ProcessRouteService(IProcessRouteRepository repository, ILogger<ProcessRouteService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProcessRouteDto> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            if (entity == null) throw new NotFoundException("ProcessRoute", id);
            return MapToDto(entity);
        }

        public async Task<ProcessRouteDto?> GetByProductIdAsync(Guid productId)
        {
            var entity = await _repository.GetActiveByProductIdAsync(productId);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<ProcessRouteDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<ProcessRouteDto> CreateAsync(CreateProcessRouteDto dto)
        {
            if (await _repository.ExistsByCodeAsync(dto.RouteCode))
                throw new AppException($"ProcessRoute with code '{dto.RouteCode}' already exists");

            var entity = new Models.ProcessRoute
            {
                ProcessRouteId = Guid.NewGuid(),
                RouteCode = dto.RouteCode,
                ProductId = dto.ProductId,
                WorkCenterId = dto.WorkCenterId,
                Description = dto.Description
            };

            foreach (var stepDto in dto.Steps)
            {
                var step = new ProcessRouteStep
                {
                    ProcessRouteStepId = Guid.NewGuid(),
                    ProcessRouteId = entity.ProcessRouteId,
                    StepNumber = stepDto.StepNumber,
                    ProcessId = stepDto.ProcessId,
                    EquipmentId = stepDto.EquipmentId,
                    SetupTimeMinutes = stepDto.SetupTimeMinutes,
                    RunTimePerUnitMinutes = stepDto.RunTimePerUnitMinutes,
                    Notes = stepDto.Notes
                };

                foreach (var matDto in stepDto.Materials)
                {
                    step.Materials.Add(new ProcessRouteStepMaterial
                    {
                        Id = Guid.NewGuid(),
                        ProcessRouteStepId = step.ProcessRouteStepId,
                        BOMLineId = matDto.BOMLineId,
                        RawMaterialId = matDto.RawMaterialId,
                        MaterialCode = matDto.MaterialCode,
                        MaterialName = matDto.MaterialName,
                        Quantity = matDto.Quantity,
                        Unit = matDto.Unit
                    });
                }
                entity.Steps.Add(step);
            }

            await _repository.CreateAsync(entity);
            _logger.LogInformation("ProcessRoute created: {Code}", dto.RouteCode);
            return await GetByIdAsync(entity.ProcessRouteId);
        }

        public async Task<ProcessRouteDto> UpdateAsync(Guid id, CreateProcessRouteDto dto)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            if (entity == null) throw new NotFoundException("ProcessRoute", id);

            entity.Description = dto.Description;
            entity.WorkCenterId = dto.WorkCenterId;

            // Clear old steps and add new
            await _repository.ClearStepsAsync(entity);

            foreach (var stepDto in dto.Steps)
            {
                var step = new ProcessRouteStep
                {
                    ProcessRouteStepId = Guid.NewGuid(),
                    ProcessRouteId = entity.ProcessRouteId,
                    StepNumber = stepDto.StepNumber,
                    ProcessId = stepDto.ProcessId,
                    EquipmentId = stepDto.EquipmentId,
                    SetupTimeMinutes = stepDto.SetupTimeMinutes,
                    RunTimePerUnitMinutes = stepDto.RunTimePerUnitMinutes,
                    Notes = stepDto.Notes
                };

                foreach (var matDto in stepDto.Materials)
                {
                    step.Materials.Add(new ProcessRouteStepMaterial
                    {
                        Id = Guid.NewGuid(),
                        ProcessRouteStepId = step.ProcessRouteStepId,
                        BOMLineId = matDto.BOMLineId,
                        RawMaterialId = matDto.RawMaterialId,
                        MaterialCode = matDto.MaterialCode,
                        MaterialName = matDto.MaterialName,
                        Quantity = matDto.Quantity,
                        Unit = matDto.Unit
                    });
                }
                entity.Steps.Add(step);
            }

            await _repository.UpdateAsync(entity);
            return await GetByIdAsync(id);
        }

        private static ProcessRouteDto MapToDto(Models.ProcessRoute r) => new()
        {
            ProcessRouteId = r.ProcessRouteId, RouteCode = r.RouteCode,
            ProductId = r.ProductId,
            WorkCenterCode = r.WorkCenter?.CenterCode ?? "",
            WorkCenterName = r.WorkCenter?.CenterName ?? "",
            Version = r.Version, IsActive = r.IsActive, Description = r.Description,
            Steps = r.Steps.OrderBy(s => s.StepNumber).Select(s => new ProcessRouteStepDto
            {
                ProcessRouteStepId = s.ProcessRouteStepId, StepNumber = s.StepNumber,
                ProcessCode = s.Process?.ProcessCode ?? "",
                ProcessName = s.Process?.ProcessName ?? "",
                EquipmentCode = s.Equipment?.EquipmentCode,
                SetupTimeMinutes = s.SetupTimeMinutes,
                RunTimePerUnitMinutes = s.RunTimePerUnitMinutes, Notes = s.Notes,
                Materials = s.Materials.Select(m => new StepMaterialDto
                {
                    RawMaterialId = m.RawMaterialId, MaterialCode = m.MaterialCode,
                    MaterialName = m.MaterialName, Quantity = m.Quantity, Unit = m.Unit
                }).ToList()
            }).ToList()
        };
    }
}
