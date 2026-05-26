using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.ProcessRoute
{
    public class ProcessRouteRepository : IProcessRouteRepository
    {
        private readonly ProductionDbContext _context;
        public ProcessRouteRepository(ProductionDbContext context) => _context = context;

        public async Task<Models.ProcessRoute?> GetByIdAsync(Guid id) =>
            await _context.ProcessRoutes.FindAsync(id);

        public async Task<Models.ProcessRoute?> GetByIdWithDetailsAsync(Guid id) =>
            await _context.ProcessRoutes
                .Include(r => r.WorkCenter)
                .Include(r => r.Steps.OrderBy(s => s.StepNumber)).ThenInclude(s => s.Process)
                .Include(r => r.Steps).ThenInclude(s => s.Equipment)
                .FirstOrDefaultAsync(r => r.ProcessRouteId == id);

        public async Task<Models.ProcessRoute?> GetActiveByProductIdAsync(Guid productId) =>
            await _context.ProcessRoutes
                .Include(r => r.WorkCenter)
                .Include(r => r.Steps.OrderBy(s => s.StepNumber)).ThenInclude(s => s.Process)
                .Include(r => r.Steps).ThenInclude(s => s.Equipment)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        public async Task<List<Models.ProcessRoute>> GetAllActiveByProductIdAsync(Guid productId) =>
            await _context.ProcessRoutes
                .Include(r => r.WorkCenter)
                .Include(r => r.Steps.OrderBy(s => s.StepNumber)).ThenInclude(s => s.Process)
                .Include(r => r.Steps).ThenInclude(s => s.Equipment)
                .Where(r => r.ProductId == productId && r.IsActive)
                .OrderBy(r => r.RouteCode)
                .ToListAsync();

        public async Task<IEnumerable<Models.ProcessRoute>> GetAllAsync() =>
            await _context.ProcessRoutes
                .Include(r => r.WorkCenter)
                .Include(r => r.Steps.OrderBy(s => s.StepNumber))
                .ThenInclude(s => s.Process)
                .Include(r=> r.Steps)
                .ThenInclude(s=>s.Equipment)
                .OrderBy(r => r.RouteCode)
                .AsNoTracking()
                .ToListAsync();



        public async Task<bool> ExistsByCodeAsync(string routeCode) =>
            await _context.ProcessRoutes.AnyAsync(r => r.RouteCode == routeCode);

        public async Task<Models.ProcessRoute> CreateAsync(Models.ProcessRoute entity)
        {
            _context.ProcessRoutes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateRouteAsync(Guid routeId, string routeCode, Guid productId, string? description, Guid workCenterId, List<DTOs.ProcessRoute.CreateProcessRouteStepDto> stepDtos)
        {
            var route = await _context.ProcessRoutes
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.ProcessRouteId == routeId);

            if (route == null) return;

            route.RouteCode = routeCode;
            route.ProductId = productId;
            route.Description = description;
            route.WorkCenterId = workCenterId;
            route.Version += 1;
            route.UpdatedAt = DateTime.UtcNow;

            // Build a lookup of existing tracked steps by their ID
            var existingStepMap = route.Steps.ToDictionary(s => s.ProcessRouteStepId);
            var matchedIds = new HashSet<Guid>();

            foreach (var dto in stepDtos)
            {
                if (dto.ProcessRouteStepId.HasValue && dto.ProcessRouteStepId.Value != Guid.Empty
                    && existingStepMap.TryGetValue(dto.ProcessRouteStepId.Value, out var tracked))
                {
                    // UPDATE existing tracked entity in-place — no new objects created
                    tracked.ProcessId = dto.ProcessId;
                    tracked.StepNumber = dto.StepNumber;
                    tracked.EquipmentId = dto.EquipmentId;
                    tracked.SetupTimeMinutes = dto.SetupTimeMinutes;
                    tracked.RunTimePerUnitMinutes = dto.RunTimePerUnitMinutes;
                    tracked.OutputMultiplier = dto.OutputMultiplier;
                    tracked.OutputUnit = dto.OutputUnit;
                    tracked.Notes = dto.Notes;
                    matchedIds.Add(tracked.ProcessRouteStepId);
                }
                else
                {
                    // ADD brand new step — do NOT set ProcessRouteStepId!
                    // EF Core has ValueGeneratedOnAdd configured, so setting a GUID explicitly
                    // causes EF to treat it as Modified (existing) instead of Added (new).
                    route.Steps.Add(new ProcessRouteStep
                    {
                        // ProcessRouteStepId left as default (Guid.Empty) — EF generates it
                        ProcessRouteId = routeId,
                        StepNumber = dto.StepNumber,
                        ProcessId = dto.ProcessId,
                        EquipmentId = dto.EquipmentId,
                        SetupTimeMinutes = dto.SetupTimeMinutes,
                        RunTimePerUnitMinutes = dto.RunTimePerUnitMinutes,
                        OutputMultiplier = dto.OutputMultiplier,
                        OutputUnit = dto.OutputUnit,
                        Notes = dto.Notes
                    });
                }
            }

            // REMOVE steps that were not in the incoming DTO list
            var stepsToRemove = route.Steps.Where(s => !matchedIds.Contains(s.ProcessRouteStepId)
                && existingStepMap.ContainsKey(s.ProcessRouteStepId)).ToList();

            foreach (var toRemove in stepsToRemove)
            {
                bool isUsed = await _context.WorkOrders.AnyAsync(w => w.ProcessRouteStepId == toRemove.ProcessRouteStepId);
                if (isUsed)
                {
                    throw new MyERP.Services.Production.Exceptions.BusinessRuleException(
                        $"Cannot remove step 'Step {toRemove.StepNumber}' because it is used by a Work Order.");
                }
                route.Steps.Remove(toRemove);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ClearStepsAsync(Models.ProcessRoute route)
        {
            // Legacy — kept for interface compatibility, but UpdateRouteAsync is preferred
            var stepIds = route.Steps.Select(s => s.ProcessRouteStepId).ToList();
            if (stepIds.Count > 0)
            {
                await _context.ProcessRouteStepMaterials
                    .Where(m => stepIds.Contains(m.ProcessRouteStepId))
                    .ExecuteDeleteAsync();
                await _context.ProcessRouteSteps
                    .Where(s => stepIds.Contains(s.ProcessRouteStepId))
                    .ExecuteDeleteAsync();
            }
            foreach (var step in route.Steps.ToList())
                _context.Entry(step).State = EntityState.Detached;
            route.Steps.Clear();
        }

        public async Task<Models.ProcessRoute> UpdateAsync(Models.ProcessRoute entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
