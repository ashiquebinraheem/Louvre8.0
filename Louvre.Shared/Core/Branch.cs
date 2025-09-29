using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class Branch : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? BranchID { get; set; }
        public string? BranchName { get; set; }
        public int? ParentBranchID { get; set; }
        public string? GoogleLocation { get; set; }
    }
}
