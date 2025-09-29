using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class RequestVehicleTracking : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? RequestVehicleTrackingID { get; set; }
        public int? RequestVehicleID { get; set; }
        public bool IsCheckOut { get; set; }
    }
}
