using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Location : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? LocationID { get; set; }
        public string? LocationName { get; set; }
        public int? LocationTypeID { get; set; }
    }
}
