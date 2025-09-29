using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class RequestTypeApprovalStage : BaseEntity
    {
        [PrimaryKey]
        public int? ApprovalStageID { get; set; }
        public int? RequestTypeID { get; set; }
        public int Stage { get; set; }
        public int? UserTypeID { get; set; }
    }
}
