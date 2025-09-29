using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class VisitorTracking : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? VisitorTrackingID { get; set; }
        public int? VisitRequestID { get; set; }
        public bool IsCheckOut { get; set; }
    }
}
