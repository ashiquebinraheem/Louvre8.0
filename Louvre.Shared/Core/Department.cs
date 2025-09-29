using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Department : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
    }
}
