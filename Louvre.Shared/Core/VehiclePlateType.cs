using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class VehiclePlateType : BaseEntity
    {
        [PrimaryKey]
        public int? VehiclePlateTypeID { get; set; }
        public string? VehiclePlateTypeName { get; set; }
    }
}
