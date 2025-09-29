using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class SlotPattern : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? SlotPatternID { get; set; }
        public string? SlotPatternName { get; set; }
    }
}
