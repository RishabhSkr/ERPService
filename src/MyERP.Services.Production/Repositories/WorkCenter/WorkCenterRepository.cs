using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.WorkCenter
{
    public class WorkCenterRepository : IWorkCenterRepository
    {
        private readonly ProductionDbContext _context;
        public WorkCenterRepository(ProductionDbContext context) => _context = context;

        public async Task<Models.WorkCenter?> GetByIdAsync(Guid id) =>
            await _context.WorkCenters.FindAsync(id);

        public async Task<Models.WorkCenter?> GetByIdWithEquipmentAsync(Guid id) =>
            await _context.WorkCenters.Include(wc => wc.Equipment)
                .FirstOrDefaultAsync(wc => wc.WorkCenterId == id);

        public async Task<IEnumerable<Models.WorkCenter>> GetAllAsync() =>
            await _context.WorkCenters.Include(wc => wc.Equipment)
                .OrderBy(wc => wc.CenterCode).ToListAsync();

        public async Task<bool> ExistsByCodeAsync(string centerCode) =>
            await _context.WorkCenters.AnyAsync(wc => wc.CenterCode == centerCode);

        public async Task<Models.WorkCenter> CreateAsync(Models.WorkCenter entity)
        {
            _context.WorkCenters.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Models.WorkCenter> UpdateAsync(Models.WorkCenter entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.WorkCenters.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.WorkCenters.FindAsync(id);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
