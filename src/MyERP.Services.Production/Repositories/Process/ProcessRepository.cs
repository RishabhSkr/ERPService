using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.Process
{
    public class ProcessRepository : IProcessRepository
    {
        private readonly ProductionDbContext _context;
        public ProcessRepository(ProductionDbContext context) => _context = context;

        public async Task<Models.Process?> GetByIdAsync(Guid id) =>
            await _context.Processes.FindAsync(id);

        public async Task<IEnumerable<Models.Process>> GetAllAsync() =>
            await _context.Processes.OrderBy(p => p.ProcessCode).ToListAsync();

        public async Task<bool> ExistsByCodeAsync(string processCode) =>
            await _context.Processes.AnyAsync(p => p.ProcessCode == processCode);

        public async Task<Models.Process> CreateAsync(Models.Process entity)
        {
            _context.Processes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Models.Process> UpdateAsync(Models.Process entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Processes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Processes.FindAsync(id);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
