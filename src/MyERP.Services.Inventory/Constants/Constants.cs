namespace MyERP.Services.Inventory.Constants
{
    public static class SystemUser
    {
        public static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
    public static class MovementType
    {
        public const string IN = "IN";
        public const string OUT = "OUT";
        public const string RESERVE = "RESERVE";
        public const string RELEASE = "RELEASE";
        public const string ADJUST = "ADJUST";
        public const string ADJUST_OUT = "ADJUST_OUT";
        public const string SCRAP = "SCRAP";
        public const string TRANSFER = "TRANSFER";
    }
    public static class ItemType
    {
        public const string PRODUCT = "Product";
        public const string RAW_MATERIAL = "RawMaterial";
    }
    public static class ReferenceType
    {
        public const string SALES_ORDER = "SalesOrder";
        public const string PRODUCTION_ORDER = "ProductionOrder";
        public const string PURCHASE = "Purchase";
    }

}