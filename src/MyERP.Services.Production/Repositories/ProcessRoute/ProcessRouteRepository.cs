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

        public async Task UpdateRouteAsync(Guid routeId,string routeCode, Guid productId, string? description, Guid workCenterId, List<ProcessRouteStep> newSteps)
        {
            // Step 1: Delete old materials and steps directly via SQL (bypass change tracker)
            await _context.ProcessRouteStepMaterials
                .Where(m => m.ProcessRouteStep != null && m.ProcessRouteStep.ProcessRouteId == routeId)
                .ExecuteDeleteAsync();
                
            await _context.ProcessRouteSteps
                .Where(s => s.ProcessRouteId == routeId)
                .ExecuteDeleteAsync();
            // Step 2: Update the route fields directly via SQL
            await _context.ProcessRoutes
                .Where(r => r.ProcessRouteId == routeId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r=>r.RouteCode,routeCode)
                    .SetProperty(r=>r.ProductId,productId)
                    .SetProperty(r => r.Description, description)
                    .SetProperty(r => r.WorkCenterId, workCenterId)
                    .SetProperty(r => r.Version, r => r.Version + 1)
                    .SetProperty(r => r.UpdatedAt, DateTime.UtcNow)
                );

            // Step 3: Add new steps (fresh, untracked entities)
            if (newSteps.Count > 0)
            {
                _context.ProcessRouteSteps.AddRange(newSteps);
                await _context.SaveChangesAsync();
            }

            // Step 4: Clear change tracker so next query gets fresh data from DB
            _context.ChangeTracker.Clear();
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
