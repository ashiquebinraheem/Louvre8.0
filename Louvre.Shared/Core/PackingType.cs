using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class PackingType : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? PackingTypeID { get; set; }
        public string? PackingTypeName { get; set; }
    }
}
