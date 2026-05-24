namespace MyERP.Services.Inventory.Models
{
    public enum WarehouseType
    {
        Physical = 0,    // Actual Godown
        System = 1,      // System Staging / Transit / Adjustment
        Scrap = 2        // Scrapping area
    }
}
