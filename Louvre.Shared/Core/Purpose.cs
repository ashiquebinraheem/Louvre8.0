using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Purpose : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? PurposeID { get; set; }
        public string? PurposeName { get; set; }
    }
}
