using Progbiz.DapperEntity;
using System;

namespace Louvre.Shared.Core
{
    public class RequestApproval : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? RequestApprovalID { get; set; }
        public int? RequestID { get; set; }
        public int? StorageLocationID { get; set; }
        public bool IsRejected { get; set; }
        public bool IsReported { get; set; }
        public string? Remarks { get; set; }
        public int? ApprovalStage { get; set; }
        public DateTime? Date { get; set; }
        public int? SlotID { get; set; }
        public int? LocationID { get; set; }
        public bool NeedHigherLevelApproval { get; set; }
        public int? DailyPassRequestID { get; set; }
    }
}
