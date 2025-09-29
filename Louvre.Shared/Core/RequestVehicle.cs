using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class RequestVehicle : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? RequestVehicleID { get; set; }
        public int? RequestID { get; set; }
        public int? EmployeeID { get; set; }
        public int? VehicleID { get; set; }
        public int PassengerCount { get; set; }
    }
}
