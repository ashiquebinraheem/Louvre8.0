using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class VehicleType : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? VehicleTypeID { get; set; }
        public string? VehicleTypeName { get; set; }
    }
}
