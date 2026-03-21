using MyERP.Services.Production.DTOs.MRP;
using MyERP.Services.Production.Repositories.ProductionOrders;
using MyERP.Services.Production.Services.External;

namespace MyERP.Services.Production.Services.MRP
{
    public class MRPService : IMRPService
    {
        private readonly IProductionOrderRepository _orderRepo;
        private readonly IInventoryServiceClient _inventoryClient;
        private readonly ILogger<MRPService> _logger;

        public MRPService(
            IProductionOrderRepository orderRepo,
            IInventoryServiceClient inventoryClient,
            ILogger<MRPService> logger)
        {
            _orderRepo = orderRepo;
            _inventoryClient = inventoryClient;
            _logger = logger;
        }

        public async Task<MRPResultDto> RunAsync(RunMRPRequestDto request)
        {
            _logger.LogInformation("Starting MRP run...");

            // ─── Step 1: Get all relevant Production Orders ───
            var allOrders = await _orderRepo.GetAllAsync();
            
            // Filter by status
            var validStatuses = request.IncludeStatuses ?? new List<string> { "Create", "Released" };
            var orders = allOrders
                .Where(o => validStatuses.Contains(o.Status, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Filter by product (optional)
            if (request.ProductId.HasValue)
                orders = orders.Where(o => o.ProductId == request.ProductId.Value).ToList();

            if (!orders.Any())
            {
                _logger.LogInformation("No production orders found for MRP run");
                return new MRPResultDto { TotalOrders = 0 };
            }

            // ─── Step 2: BOM Explosion — collect all material needs ───
            // Group MaterialRequirements by RawMaterialId across all orders
            var materialGroups = orders
                .Where(o => o.MaterialRequirements != null)
                .SelectMany(o => o.MaterialRequirements.Select(mr => new
                {
                    mr.RawMaterialId,
                    mr.MaterialCode,
                    mr.MaterialName,
                    mr.QuantityRequired,
                    mr.Unit,
                    OrderNumber = o.OrderNumber
                }))
                .GroupBy(m => m.RawMaterialId)
                .ToList();

            // ─── Step 3: For each material, check inventory stock ───
            var materialRequirements = new List<MaterialRequirementLineDto>();
            var purchaseSuggestions = new List<PurchaseSuggestionDto>();

            foreach (var group in materialGroups)
            {
                var first = group.First();
                var grossRequired = group.Sum(m => m.QuantityRequired);
                var usedInOrders = group.Select(m => m.OrderNumber).Distinct().ToList();

                // HTTP call to Inventory Service for stock levels
                decimal currentStock = 0;
                decimal reservedStock = 0;
                decimal availableStock = 0;

                try
                {
                    var material = await _inventoryClient.GetRawMaterialByIdAsync(group.Key);
                    if (material != null)
                    {
                        availableStock = material.AvailableStock;
                        currentStock = material.CurrentStock;
                        reservedStock = material.ReservedStock;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch stock for {MaterialCode}", first.MaterialCode);
                }

                // ─── Step 4: Calculate Net Requirement ───
                var netRequirement = grossRequired - availableStock;
                if (netRequirement < 0) netRequirement = 0;

                var line = new MaterialRequirementLineDto
                {
                    RawMaterialId = group.Key,
                    MaterialCode = first.MaterialCode,
                    MaterialName = first.MaterialName,
                    Unit = first.Unit,
                    GrossRequirement = grossRequired,
                    CurrentStock = currentStock,
                    ReservedStock = reservedStock,
                    AvailableStock = availableStock,
                    NetRequirement = netRequirement,
                    NeedsPurchase = netRequirement > 0,
                    UsedInOrders = usedInOrders
                };
                materialRequirements.Add(line);

                // ─── Step 5: Purchase Suggestion ───
                if (netRequirement > 0)
                {
                    // Priority based on shortage percentage
                    var shortagePercent = availableStock > 0
                        ? (netRequirement / grossRequired) * 100
                        : 100;

                    var priority = shortagePercent switch
                    {
                        >= 80 => "Critical",
                        >= 50 => "High",
                        >= 20 => "Medium",
                        _ => "Low"
                    };

                    purchaseSuggestions.Add(new PurchaseSuggestionDto
                    {
                        RawMaterialId = group.Key,
                        MaterialCode = first.MaterialCode,
                        MaterialName = first.MaterialName,
                        QuantityToOrder = netRequirement,
                        Unit = first.Unit,
                        Priority = priority
                    });
                }
            }

            var result = new MRPResultDto
            {
                RunDate = DateTime.UtcNow,
                TotalOrders = orders.Count,
                TotalProducts = orders.Select(o => o.ProductId).Distinct().Count(),
                MaterialRequirements = materialRequirements.OrderByDescending(m => m.NetRequirement).ToList(),
                PurchaseSuggestions = purchaseSuggestions.OrderByDescending(p => p.Priority).ToList()
            };

            _logger.LogInformation("MRP run complete: {Orders} orders, {Materials} materials, {Suggestions} purchase suggestions",
                result.TotalOrders, materialRequirements.Count, purchaseSuggestions.Count);

            return result;
        }
    }
}
