using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class RequestMeterialType : BaseEntity
    {
        [PrimaryKey]
        public int? MeterialTypeID { get; set; }
        public string? MeterialTypeName { get; set; }
    }
}
