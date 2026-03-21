namespace MyERP.Services.Production.Models
{
    /// <summary>
    /// EquipmentProcess — junction table: which equipment can handle which process.
    /// One equipment can do multiple processes, one process can run on multiple equipment.
    /// </summary>
    public class EquipmentProcess
    {
        public Guid Id { get; set; }
        
        public Guid EquipmentId { get; set; }
        public virtual Equipment? Equipment { get; set; }
        
        public Guid ProcessId { get; set; }
        public virtual Process? Process { get; set; }
    }
}
