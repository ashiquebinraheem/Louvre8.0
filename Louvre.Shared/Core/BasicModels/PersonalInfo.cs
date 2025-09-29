using Progbiz.DapperEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    [TableName("PersonalInfos")]
    public class PersonalInfo : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? PersonalInfoID { get; set; }
        public string? Honorific { get; set; }
        [Required]
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? NickName { get; set; }
        public int? GenderID { get; set; }
        public int? ProfileImageMediaID { get; set; }

        [MaxLength(10)]
        [RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? Phone1 { get; set; }
        [MaxLength(10)]
        [RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? Phone2 { get; set; }
        public string? Email1 { get; set; }
        public DateTime? DOB { get; set; }
    }




    [TableName("PersonalInfos")]
    public class PersonalInfo_Client : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? PersonalInfoID { get; set; }
        //[Required]
        public string? FirstName { get; set; }
        public int? ProfileImageMediaID { get; set; }
        public string? Email1 { get; set; }

        [MaxLength(10)]
        [Required(ErrorMessage = "You must provide a phone number")]
        [Display(Name = "Home Phone")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? Phone1 { get; set; }
        [MaxLength(10)]
        [RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? Phone2 { get; set; }
    }


}
