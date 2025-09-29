using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Module : BaseEntity
    {
        [PrimaryKey]
        public int? ModuleID { get; set; }
        public string? ModuleName { get; set; }
        public string? PermissionName { get; set; }
    }
}
