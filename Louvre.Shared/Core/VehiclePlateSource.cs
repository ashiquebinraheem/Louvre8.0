using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class VehiclePlateSource : BaseEntity
    {
        [PrimaryKey]
        public int? VehiclePlateSourceID { get; set; }
        public string? VehiclePlateSourceName { get; set; }
    }
}
