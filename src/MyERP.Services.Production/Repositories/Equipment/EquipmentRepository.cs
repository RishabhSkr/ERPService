using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.Equipment
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly ProductionDbContext _context;
        public EquipmentRepository(ProductionDbContext context) => _context = context;

        public async Task<Models.Equipment?> GetByIdAsync(Guid id) =>
            await _context.Equipment.FindAsync(id);

        public async Task<Models.Equipment?> GetByIdWithWorkCenterAsync(Guid id) =>
            await _context.Equipment.Include(e => e.WorkCenter)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

        public async Task<IEnumerable<Models.Equipment>> GetAllAsync() =>
            await _context.Equipment.Include(e => e.WorkCenter)
                .OrderBy(e => e.EquipmentCode).ToListAsync();

        public async Task<IEnumerable<Models.Equipment>> GetByWorkCenterAsync(Guid workCenterId) =>
            await _context.Equipment.Include(e => e.WorkCenter)
                .Where(e => e.WorkCenterId == workCenterId)
                .OrderBy(e => e.EquipmentCode).ToListAsync();

        public async Task<bool> ExistsByCodeAsync(string equipmentCode) =>
            await _context.Equipment.AnyAsync(e => e.EquipmentCode == equipmentCode);

        public async Task<Models.Equipment> CreateAsync(Models.Equipment entity)
        {
            _context.Equipment.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Models.Equipment> UpdateAsync(Models.Equipment entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Equipment.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Equipment.FindAsync(id);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // EquipmentProcess mapping
        public async Task<IEnumerable<EquipmentProcess>> GetLinkedProcessesAsync(Guid equipmentId) =>
            await _context.EquipmentProcesses
                .Include(ep => ep.Process)
                .Where(ep => ep.EquipmentId == equipmentId).ToListAsync();

        public async Task ReplaceLinkedProcessesAsync(Guid equipmentId, List<Guid> processIds)
        {
            var existing = await _context.EquipmentProcesses
                .Where(ep => ep.EquipmentId == equipmentId).ToListAsync();
            _context.EquipmentProcesses.RemoveRange(existing);

            foreach (var processId in processIds)
            {
                _context.EquipmentProcesses.Add(new EquipmentProcess
                {
                    Id = Guid.NewGuid(),
                    EquipmentId = equipmentId,
                    ProcessId = processId
                });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanPerformProcessAsync(Guid equipmentId, Guid processId) =>
            await _context.EquipmentProcesses
                .AnyAsync(ep => ep.EquipmentId == equipmentId && ep.ProcessId == processId);
    }
}
