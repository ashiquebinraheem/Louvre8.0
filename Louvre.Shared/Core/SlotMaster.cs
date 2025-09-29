using Progbiz.DapperEntity;
using System;
namespace Louvre.Shared.Core
{
    public class SlotMaster : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? SlotMasterID { get; set; }
        public DateTime? Date { get; set; }
        public int? SlotGroupID { get; set; }
        public int? BranchID { get; set; }
    }
}
