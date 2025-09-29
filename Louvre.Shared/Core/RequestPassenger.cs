
using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class RequestPassenger : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? PassengerID { get; set; }
        public int? RequestID { get; set; }
        public int? EmployeeID { get; set; }
        public int? DailyPassRequestID { get; set; }
    }
}
