using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Duration : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? DurationID { get; set; }
        public string? DurationName { get; set; }
        public int Minutes { get; set; }
    }
}
