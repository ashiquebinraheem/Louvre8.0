using Progbiz.DapperEntity;
namespace Louvre.Shared.Core
{
    public class EmployeeDesignation : BaseEntity
    {
        [PrimaryKey]
        public int? DesignationID { get; set; }
        public string? DesignationName { get; set; }
    }
}
