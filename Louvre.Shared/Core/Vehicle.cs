using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class Vehicle : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? VehicleID { get; set; }
        public string? RegisterNo { get; set; }
        public int? VehicleTypeID { get; set; }
        public string? VehicleSize { get; set; }
        public string? PlateNo { get; set; }
        public int? VehicleMakeID { get; set; }
        public int? VehiclePlateSourceID { get; set; }
        public int? VehiclePlateTypeID { get; set; }
        public int? VehiclePlateCategoryID { get; set; }
    }
}
