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
                .Include(r => r.Steps).ThenInclude(s => s.Materials)
                .FirstOrDefaultAsync(r => r.ProcessRouteId == id);

        public async Task<Models.ProcessRoute?> GetActiveByProductIdAsync(Guid productId) =>
            await _context.ProcessRoutes
                .Include(r => r.WorkCenter)
                .Include(r => r.Steps.OrderBy(s => s.StepNumber)).ThenInclude(s => s.Process)
                .Include(r => r.Steps).ThenInclude(s => s.Equipment)
                .Include(r => r.Steps).ThenInclude(s => s.Materials)
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        public async Task<IEnumerable<Models.ProcessRoute>> GetAllAsync() =>
            await _context.ProcessRoutes
                .Include(r => r.WorkCenter)
                .Include(r => r.Steps.OrderBy(s => s.StepNumber)).ThenInclude(s => s.Process)
                .Include(r => r.Steps).ThenInclude(s => s.Materials)
                .OrderBy(r => r.RouteCode).ToListAsync();

        public async Task<bool> ExistsByCodeAsync(string routeCode) =>
            await _context.ProcessRoutes.AnyAsync(r => r.RouteCode == routeCode);

        public async Task<Models.ProcessRoute> CreateAsync(Models.ProcessRoute entity)
        {
            _context.ProcessRoutes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Models.ProcessRoute> UpdateAsync(Models.ProcessRoute entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ProcessRoutes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task ClearStepsAsync(Models.ProcessRoute route)
        {
            _context.ProcessRouteStepMaterials.RemoveRange(route.Steps.SelectMany(s => s.Materials));
            _context.ProcessRouteSteps.RemoveRange(route.Steps);
            await _context.SaveChangesAsync();
        }
    }
}
