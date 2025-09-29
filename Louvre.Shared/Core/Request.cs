using Progbiz.DapperEntity;
using System;

namespace Louvre.Shared.Core
{
    public class Request : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? RequestID { get; set; }
        public DateTime? Date { get; set; }
        public int? EmployeeID { get; set; }
        public int? BranchID { get; set; }
        public int? SubBranchID { get; set; }
        public int? SlotID { get; set; }
        public int? RequestModeID { get; set; }
        public int? LocationID { get; set; }
        public bool ContainsExplosive { get; set; }
        public bool IsDisposalRequired { get; set; }
        public int? MeterialTypeID { get; set; }
        public int? StorageLocationID { get; set; } 
        public string? HostEmail { get; set; }
        public string? Narration { get; set; }
        public int RequestNo { get; set; }
        public bool FromApp { get; set; }
        public bool IsProjectAsset { get; set; }
        public string? PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? POOwnerID { get; set; }
        public int RefID { get; set; }

        public bool ContainsCarryItem { get; set; }
    }
}
