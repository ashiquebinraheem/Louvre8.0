using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class Country : BaseEntity
    {
        [PrimaryKey]
        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? Capital { get; set; }
        public string? Code2 { get; set; }
        public string? Code3 { get; set; }
        public string? ISDCode { get; set; }
        public string? TimeZone { get; set; }
        public int TimeZoneMinutes { get; set; }
        public bool Show { get; set; }
    }
}
