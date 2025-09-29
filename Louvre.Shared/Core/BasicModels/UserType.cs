using Progbiz.DapperEntity;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    [TableName("UserTypes")]
    public class UserType : BaseEntity
    {
        [PrimaryKey]
        public int UserTypeID { get; set; }
        [Required]
        public string? UserTypeName { get; set; }
        public string? DisplayName { get; set; }
        public int? PriorityOrder { get; set; }
        public int UserNature { get; set; }
        public bool ShowInList { get; set; }
        public string? Email { get; set; }
    }
}
