using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class UserModule : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? UserModuleID { get; set; }
        public int? UserID { get; set; }
        public int? ModuleID { get; set; }
        public bool CanAccess { get; set; }
    }
}
