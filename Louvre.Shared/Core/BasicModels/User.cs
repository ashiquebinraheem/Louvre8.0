using Progbiz.DapperEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    [TableName("Users")]
    public class User : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? UserID { get; set; }
        //[Required]
        public string? UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string? Password { get; set; }
        public string? Salt { get; set; }
        [Required]
        public int UserTypeID { get; set; }
        public int? PersonalInfoID { get; set; }
        public bool LoginStatus { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? SecurityStamp { get; set; }
        public DateTime? SecurityStampIssuedOn { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public int? HostID { get; set; }
        public string? Justification { get; set; }
    }



    [TableName("Users")]
    public class User_Profile : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? UserID { get; set; }
        public int? PersonalInfoID { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
    }
}