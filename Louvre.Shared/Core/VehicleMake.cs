using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class VehicleMake : BaseEntity
    {
        [PrimaryKey]
        public int? VehicleMakeID { get; set; }
        public string? VehicleMakeName { get; set; }
    }
}
