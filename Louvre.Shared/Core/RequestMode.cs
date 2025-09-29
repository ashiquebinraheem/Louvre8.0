using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class RequestMode : BaseEntity
    {
        [PrimaryKey]
        public int RequestModeID { get; set; }
        public string? ModeName { get; set; }
        public bool NeedMeterial { get; set; }
        public int? LocationTypeID { get; set; }
        public bool IsIn { get; set; }
    }
}
