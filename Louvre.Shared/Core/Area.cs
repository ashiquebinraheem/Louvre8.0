using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Area : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? AreaID { get; set; }
        public string? AreaName { get; set; }
    }
}
