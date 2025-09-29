using Progbiz.DapperEntity;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    public class Company : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? CompanyID { get; set; }
        public string? CompanyName { get; set; }
        //[Required]
        public int? PersonalInfoID { get; set; }
        public bool IsBlackListed { get; set; }
        public string? BlacklistReason { get; set; }
        public int? VendorID { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPersonNumber { get; set; }
        public string? CompanyAddress { get; set; }
    }
}
