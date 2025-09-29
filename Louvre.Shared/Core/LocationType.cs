using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class LocationType : BaseEntity
    {
        [PrimaryKey]
        public int? LocationTypeID { get; set; }
        public string? LocationTypeName { get; set; }
    }
}
