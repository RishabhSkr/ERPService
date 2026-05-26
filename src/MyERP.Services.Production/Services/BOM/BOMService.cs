using MyERP.Services.Production.DTOs.BOM;
using MyERP.Services.Production.Exceptions;
using MyERP.Services.Production.Models;
using MyERP.Services.Production.Repositories.BOM;
using MyERP.Services.Production.Services.External;

namespace MyERP.Services.Production.Services.BOM
{
    public class BOMService : IBOMService
    {
        private readonly IBOMRepository _repository;
        private readonly ILogger<BOMService> _logger;
        private readonly IInventoryServiceClient _inventoryClient;
        public BOMService(IBOMRepository repository, ILogger<BOMService> logger, IInventoryServiceClient inventoryClient)
        {
            _repository = repository;
            _logger = logger;
            _inventoryClient = inventoryClient;
        }

        public async Task<BOMDto> GetByIdAsync(Guid bomId)
        {
            var bom = await _repository.GetByIdWithLinesAsync(bomId);
            if (bom == null)
                throw new NotFoundException("BOM", bomId);
            
            return MapToDto(bom);
        }

        public async Task<BOMDto?> GetActiveByProductIdAsync(Guid productId)
        {
            var bom = await _repository.GetActiveByProductIdAsync(productId);
            return bom == null ? null : MapToDto(bom);
        }

        public async Task<IEnumerable<BOMDto>> GetAllAsync()
        {
            var boms = await _repository.GetAllAsync();
            return boms.Select(MapToDto);
        }

        public async Task<BOMDto> CreateAsync(CreateBOMDto dto, Guid? userId = null)
        {
            // Check if BOMCode already exists
            if (await _repository.ExistsByCodeAsync(dto.BOMCode))
                throw new ConflictException($"BOM with code '{dto.BOMCode}' already exists");
            // ✅ NEW: Validate materials exist in Inventory
            foreach (var line in dto.Lines)
            {
                var material = await _inventoryClient.GetRawMaterialByIdAsync(line.RawMaterialId);
                if (material == null)
                    throw new NotFoundException($"Material '{line.MaterialCode}' not found in Inventory");
                if (!material.IsActive)
                    throw new BusinessRuleException($"Material '{material.MaterialName}' is not active");
            }
            var bom = new Models.BOM
            {
                BOMId = Guid.NewGuid(),
                ProductId = dto.ProductId,
                BomCode = dto.BOMCode,
                ProductName = dto.ProductName,
                Description = dto.Description,
                Version = 1,
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Lines = dto.Lines.Select((line, index) => new BOMLine
                {
                    BOMLineId = Guid.NewGuid(),
                    LineNumber = index + 1,
                    RawMaterialId = line.RawMaterialId,
                    MaterialCode = line.MaterialCode,
                    MaterialName = line.MaterialName,
                    Quantity = line.Quantity,
                    Unit = line.Unit,
                    ScrapPercentage = line.ScrapPercentage,
                    ProcessId = line.ProcessId,
                    ProcessCode = line.ProcessCode,
                    ProcessName = line.ProcessName
                }).ToList()
            };

            await _repository.CreateAsync(bom);
            
            _logger.LogInformation("Created BOM: {BomCode} for product {ProductId}", bom.BomCode, bom.ProductId);
            
            return MapToDto(bom);
        }

        public async Task<BOMDto> UpdateAsync(Guid bomId, UpdateBOMDto dto)
        {
            // ================================================================
            // FIX: EF Core concurrency issue
            //
            // OLD (BROKEN):
            //   bom.Lines.Clear();           ← EF marks old lines for DELETE
            //   bom.Lines.Add(new BOMLine    ← Same PK → DELETE + INSERT conflict
            //   { BOMLineId = line.BOMLineId ?? NewGuid() });
            //   SaveChanges → "affected 0 rows" exception!
            //
            // NEW (CORRECT):
            //   1. Update BOM header separately (no Lines touch)
            //   2. ReplaceLinesAsync = explicit DELETE from DB + INSERT fresh rows
            //      → No EF tracking confusion, no shared PKs
            // ================================================================

            var bom = await _repository.GetByIdAsync(bomId);  // no .Include(Lines) → clean context
            if (bom == null)
                throw new NotFoundException("BOM", bomId);

            // Step 1: Update header fields only
            if (dto.Description != null)
                bom.Description = dto.Description;

            if (dto.Lines != null)
                bom.Version++;  // Auto-increment version when lines change

            bom.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(bom);  // Only updates BOM header — no Lines

            // Step 2: Replace lines atomically (if lines provided)
            if (dto.Lines != null)
            {
                var newLines = dto.Lines.Select((line, index) => new BOMLine
                {
                    BOMLineId = Guid.NewGuid(),  // ALWAYS new GUID — never reuse old PK!
                    BOMId = bomId,
                    LineNumber = index + 1,
                    RawMaterialId = line.RawMaterialId,
                    MaterialCode = line.MaterialCode,
                    MaterialName = line.MaterialName,
                    Quantity = line.Quantity,
                    Unit = line.Unit,
                    ScrapPercentage = line.ScrapPercentage,
                    ProcessId = line.ProcessId,
                    ProcessCode = line.ProcessCode,
                    ProcessName = line.ProcessName
                }).ToList();

                await _repository.ReplaceLinesAsync(bomId, newLines);
            }

            return MapToDto(await _repository.GetByIdWithLinesAsync(bomId) ?? throw new NotFoundException("BOM", bomId));
        }

        public async Task DeactivateAsync(Guid bomId)
        {
            var bom = await _repository.GetByIdAsync(bomId);
            if (bom == null)
                throw new NotFoundException("BOM", bomId);

            bom.IsActive = false;
            bom.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(bom);
            
            _logger.LogInformation("Deactivated BOM: {BomCode}", bom.BomCode);
        }

        private static BOMDto MapToDto(Models.BOM bom)
        {
            return new BOMDto
            {
                BOMId = bom.BOMId,
                ProductId = bom.ProductId,
                BOMCode = bom.BomCode,
                ProductName = bom.ProductName,
                Version = bom.Version,
                IsActive = bom.IsActive,
                Description = bom.Description,
                CreatedAt = bom.CreatedAt,
                Lines = bom.Lines.Select(l => new BOMLineDto
                {
                    BOMLineId = l.BOMLineId,
                    LineNumber = l.LineNumber,
                    RawMaterialId = l.RawMaterialId,
                    MaterialCode = l.MaterialCode,
                    MaterialName = l.MaterialName,
                    Quantity = l.Quantity,
                    Unit = l.Unit,
                    ScrapPercentage = l.ScrapPercentage,
                    ProcessId = l.ProcessId,
                    ProcessCode = l.ProcessCode,
                    ProcessName = l.ProcessName
                }).ToList()
            };
        }
    }
}
