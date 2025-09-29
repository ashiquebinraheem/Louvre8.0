using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class SlotPatternItem : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? SlotPatternItemID { get; set; }
        public int? SlotPatternID { get; set; }
        public int DayNo { get; set; }
        public int? SlotGroupID { get; set; }
    }
}
