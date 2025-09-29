using System.Collections.Generic;
using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class SlotGroupItem : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? SlotGroupItemID { get; set; }
        public int? SlotGroupID { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public int RequestCount { get; set; }

       


    }
}
