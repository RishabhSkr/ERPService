namespace MyERP.Services.Identity
{
    /// <summary>
    /// System-level constants — single source of truth for all hardcoded values.
    /// Import these everywhere instead of using magic strings/GUIDs directly.
    /// </summary>
    public static class SystemConstants
    {
        // ═══════════════════════════════════════════════════
        // SYSTEM ROLE IDs (seeded in AppDbContext)
        // ═══════════════════════════════════════════════════
        public static readonly Guid SuperAdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid PendingRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // ═══════════════════════════════════════════════════
        // SYSTEM USER IDs (seeded in AppDbContext)
        // ═══════════════════════════════════════════════════
        public static readonly Guid SuperAdminUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // ═══════════════════════════════════════════════════
        // ROLE NAMES
        // ═══════════════════════════════════════════════════
        public const string RoleSuperAdmin = "SuperAdmin";
        public const string RolePending = "Pending";

        // ═══════════════════════════════════════════════════
        // USER STATUS VALUES
        // ═══════════════════════════════════════════════════
        public const string StatusPending = "Pending";
        public const string StatusActive = "Active";
        public const string StatusSuspended = "Suspended";
    }
}
