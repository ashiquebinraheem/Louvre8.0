using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class VehiclePlateCategory : BaseEntity
    {
        [PrimaryKey]
        public int? VehiclePlateCategoryID { get; set; }
        public string? VehiclePlateCategoryName { get; set; }
        public int Type { get; set; }
    }
}
