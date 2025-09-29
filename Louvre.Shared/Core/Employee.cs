using Progbiz.DapperEntity;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    public class Employee : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public int? DesignationID { get; set; }
        [Required(ErrorMessage ="Please Select Company")]
        public int? CompanyID { get; set; }
        public string? Email { get; set; }
        [MaxLength(10)]
        [RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? ContactNumber { get; set; }
        public bool IsDriver { get; set; }
        public int? PersonalInfoID { get; set; }
        public int? CountryID { get; set; }
        public string? QRCode { get; set; }
    }
}
