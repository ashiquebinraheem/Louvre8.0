using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class RequestType : BaseEntity
    {
        [PrimaryKey]
        public int? RequestTypeID { get; set; }
        public string? RequestTypeName { get; set; }
    }
}
