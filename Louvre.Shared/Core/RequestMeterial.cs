using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class RequestMeterial : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? RequestMeterialID { get; set; }
        public int? RequestID { get; set; }
        public string? Description { get; set; }
        public string? Quantity { get; set; }
        public string? Weight { get; set; }
        public int? PackingTypeID { get; set; }
        public int? DailyPassRequestID { get; set; }
    }
}
