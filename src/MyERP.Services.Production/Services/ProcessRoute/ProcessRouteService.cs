using MyERP.Services.Production.DTOs.ProcessRoute;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.ProcessRoute;
using System.Text.Json;

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
            
            //  // 1. Mapping karien
            // var result = entities.Select(MapToDto);
            // // 2. Debugging ke liye JSON mein convert karein
            // var options = new JsonSerializerOptions 
            // { 
            //     WriteIndented = true, // Isse JSON sundar (readable) dikhega
            //     ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles 
            // };
            // string jsonString = JsonSerializer.Serialize(result, options);
            // // 3. Print karein
            // Console.WriteLine("--- START PROCESS ROUTE DATA ---");
            // Console.WriteLine(jsonString);
            // Console.WriteLine("--- END PROCESS ROUTE DATA ---");
            // return result;

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
                    OutputMultiplier = stepDto.OutputMultiplier,
                    OutputUnit = stepDto.OutputUnit,
                    Notes = stepDto.Notes
                };
                // Materials removed — now defined in BOM Line (industry standard)
                entity.Steps.Add(step);
            }

            await _repository.CreateAsync(entity);
            _logger.LogInformation("ProcessRoute created: {Code}", dto.RouteCode);
            return await GetByIdAsync(entity.ProcessRouteId);
        }

        public async Task<ProcessRouteDto> UpdateAsync(Guid id, CreateProcessRouteDto dto)
        {
            // Single repository call: loads route fresh, modifies tracked entities in-place, saves.
            // IMPORTANT: Do NOT pre-load the entity here (e.g. via GetByIdWithDetailsAsync),
            // because that pollutes the EF change tracker and causes DbUpdateConcurrencyException.
            await _repository.UpdateRouteAsync(id, dto.RouteCode, dto.ProductId, dto.Description, dto.WorkCenterId, dto.Steps);

            var result = await GetByIdAsync(id);
            if (result == null) throw new NotFoundException("ProcessRoute", id);
            return result;
        }

        private static ProcessRouteDto MapToDto(Models.ProcessRoute r) => new()
        {
            ProcessRouteId = r.ProcessRouteId, RouteCode = r.RouteCode,
            ProductId = r.ProductId,
            WorkCenterId = r.WorkCenterId,
            WorkCenterCode = r.WorkCenter?.CenterCode ?? "",
            WorkCenterName = r.WorkCenter?.CenterName ?? "",
            Version = r.Version, IsActive = r.IsActive, Description = r.Description,
            Steps = r.Steps.OrderBy(s => s.StepNumber).Select(s => new ProcessRouteStepDto
            {
                ProcessRouteStepId = s.ProcessRouteStepId, StepNumber = s.StepNumber,
                ProcessId = s.ProcessId,
                ProcessCode = s.Process?.ProcessCode ?? "",
                ProcessName = s.Process?.ProcessName ?? "",
                EquipmentId = s.EquipmentId,
                EquipmentCode = s.Equipment?.EquipmentCode,
                EquipmentName = s.Equipment?.EquipmentName,
                SetupTimeMinutes = s.SetupTimeMinutes,
                RunTimePerUnitMinutes = s.RunTimePerUnitMinutes,
                OutputMultiplier = s.OutputMultiplier,
                OutputUnit = s.OutputUnit,
                Notes = s.Notes,
                // Materials removed — now defined in BOM Line
            }).ToList()
        };

    }
}
