using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class SlotGroup : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? SlotGroupID { get; set; }
        public string? SlotGroupName { get; set; }
    }
}
