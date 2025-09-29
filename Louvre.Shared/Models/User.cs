using System;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Models
{

    public class UserViewModel
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Salt { get; set; }
        public int? UserTypeID { get; set; }
        public int? PersonalInfoID { get; set; }
        public bool? LoginStatus { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public bool? EmailConfirmed { get; set; }
        public string? SecurityStamp { get; set; }
        public DateTime? SecurityStampIssuedOn { get; set; }
        public string? Name { get; set; }
        public string? ProfileImageFileName { get; set; }
        public string? UserTypeName { get; set; }
        public int UserStage { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
    }


    public class UserPostViewModel
    {
        [Key]
        public int? UserID { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }
        [Required]
        public int UserTypeID { get; set; }
        public int? PersonalInfoID { get; set; }
        public bool LoginStatus { get; set; }
        public string? IMEI { get; set; }
    }


    public class UserListViewModel
    {
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? UserTypeName { get; set; }
        public string? Branches { get; set; }
    }


    public class ChangePasswordPostModel
    {
        [Required(ErrorMessage = "Current Password is required")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }
    }

    public class UserApproveListViewModel
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public string? AddedOn { get; set; }
    }
}
