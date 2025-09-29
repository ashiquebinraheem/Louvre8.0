using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Models
{
    public class RequesterPostViewModel
    {
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        [MaxLength(10)]
        [RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? ContactNumber { get; set; }
        public int CompanyID { get; set; }
        public int? CountryID { get; set; }
    }

    public class RequesterSaveResponseViewModel : BaseResponse
    {
        public List<RequesterPostViewModel> Employees { get; set; }
        public int? NewEmployeeID { get; set; }
    }


    public class UserModulePostViewModel
    {
        public int? UserModuleID { get; set; }
        public int? ModuleID { get; set; }
        public string? ModuleName { get; set; }
        public bool CanAccess { get; set; }
    }
}
