using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class Slot : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? SlotID { get; set; }
        public int? SlotMasterID { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public int RequestCount { get; set; }
    }
}
