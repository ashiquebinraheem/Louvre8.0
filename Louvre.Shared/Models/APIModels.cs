using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Models
{
    public class LoginRequestModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string? Password { get; set; }
    }



    public class LoginResponseModel: APIBaseResponse
    {
        public string? AccessToken { get; set; }
    }


    public class RegisterPostModel
    {
        [Required]
        public string? FullName { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public string? EmailId { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string? Password { get; set; }
        public string? CompanyName { get; set; }
        public string? DesignationName { get; set; }
        [Required]
        public string? HostDetail { get; set; }

        public int Type { get; set; }

        public List<DocumentUploadModel> Documents { get; set; }

    }

    public class FileUploadModel
    {
        public byte[] Content { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
    }

    public class FileUploadAPIModel
    {
        public int? MediaID { get; set; }
        public byte[] Content { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
        public string? FolderName { get; set; }
    }

    public class HomeDetail:APIBaseResponse
    {
        public int TotalVisitCount { get; set; }
        public int ApprovedVisitCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public List<IdnValuePair> DepartmentArray { get; set; }
        public List<IdnValuePair> AreaArray { get; set; }
        public List<IdnValuePair> PurposeArray { get; set; }
        public List<IdnValuePair> DurationArray { get; set; }
        public List<ViVehicle> AddedvehicleArray { get; set; }
        public List<RequesterPostViewModel> VisiterProfileArray { get; set; }
        public int SlotBefore { get; set; }
        public List<VisitRequestListViewModel> History { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }
        public string? Name { get; set; }
        public string? QRCode { get; set; }
    }

    public class MeterialHomeDetail : APIBaseResponse
    {
        public int TotalVisitCount { get; set; }
        public int ApprovedVisitCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }

        //public List<VisitRequestListViewModel> History { get; set; }
        //public List<DocumentPostViewModel> Documents { get; set; }
        public string? Name { get; set; }
        public string? QRCode { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public string? CompanyName { get; set; }

        public List<MainBranchModel> CompanyArray { get; set; } = new List<MainBranchModel>();
        public IEnumerable<RequestMode> RequestmodeArray { get; set; }
        public IEnumerable<LocationModel> LocationArray { get; set; }
        public IEnumerable<IdnValuePair> MeterialTypeArray { get; set; }
        public int SlotBefore { get; set; }
        public IEnumerable<IdnValuePair> PackingTypes { get; set; }
        public IEnumerable<IdnValuePair> VehicleTypes { get; set; }
        public IEnumerable<IdnValuePair> AddedCoPassengers { get; set; }
        public IEnumerable<string> AddedDesignations { get; set; }
        public IEnumerable<string> AddedCompanies { get; set; }
        public List<IdnValuePair> AddedvehicleArray { get; set; }
        public List<RequestListViewModel> History { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }
    }

    public class MainBranchModel
    {
        public int BranchID { get; set; }
        public string? BranchName { get; set; }
        public List<SubBranchModel> SubBranches { get; set; } = new List<SubBranchModel>();
    }

    public class SubBranchModel
    {
        public int BranchID { get; set; }
        public string? BranchName { get; set; }
    }

    public class GetSlotPostModel
    {
        [Required]
        public int? BranchID { get; set; }
        public DateTime Date { get; set; }
    }

    public class VisitRequestPostModel
    {
        public int? VisitRequestID { get; set; }
        [Required]
        public int? DeartmentId { get; set; }
        [Required]
        public int? AreaId { get; set; }
        public string? Remarks { get; set; }
        public int Purpose { get; set; }
        public int Duration { get; set; }
        public int? VehicleId { get; set; }
        public string? HostDetail { get; set; }
        [Required]
        public DateTime MeetingDate { get; set; }
        public List<DocumentUploadModel> Documents { get; set; }

    }


    public class VisitRequestViewModel
    {
        public int? VisitRequestID { get; set; }
        [Required]
        public int? DeartmentId { get; set; }
        [Required]
        public int? AreaId { get; set; }
        public string? Remarks { get; set; }
        public int Purpose { get; set; }
        public int Duration { get; set; }
        public int? VehicleId { get; set; }
        public string? HostDetail { get; set; }
        [Required]
        public DateTime MeetingDate { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }

    }

    public class MeterialRequestPostModel
    {
        public int RequestID { get; set; }
        public string? HostEmail { get; set; }
        public int RequestNo { get; set; }
        public int CompanyId { get; set; }
        public int SubCompanyId { get; set; }
        public int RequestMode { get; set; }
        public DateTime? VisitDate { get; set; }
        public int TimeSlot { get; set; }
        public int Location { get; set; }
        public int DriverId { get; set; }
        public List<int> CoPassangerId { get; set; }
        public int VehicleId { get; set; }

        public bool ContainsExplosive { get; set; }
        public bool IsDisposalRequired { get; set; }
        public int? MeterialType { get; set; }
        public string? Narration { get; set; }

        public List<RequestMeterialPostModel> Meterials { get; set; }
        //public List<RequestPassenger> Passengers { get; set; }

        public List<DocumentUploadModel> Documents { get; set; }
    }

    public class MeterialRequestViewModel
    {
        public int RequestID { get; set; }
        public string? HostEmail { get; set; }
        public int RequestNo { get; set; }
        public int CompanyId { get; set; }
        public int SubCompanyId { get; set; }
        public int RequestMode { get; set; }
        public DateTime? VisitDate { get; set; }
        public int TimeSlot { get; set; }
        public int Location { get; set; }
        public int DriverId { get; set; }
        public List<int> CoPassangerId { get; set; }
        public int VehicleId { get; set; }

        public bool ContainsExplosive { get; set; }
        public bool IsDisposalRequired { get; set; }
        public int MeterialType { get; set; }
        public string? Narration { get; set; }

        public List<RequestMeterialPostModel> Meterials { get; set; }
        //public List<RequestPassenger> Passengers { get; set; }

        public List<DocumentPostViewModel> Documents { get; set; }
    }

    public class SecurityHomeDetail:APIBaseResponse
    {
        public int ActiveVisitorCount { get; set; }
        public int CheckoutCount { get; set; }
        public int TotalVisitCount { get; set; }
        public string? Name { get; set; }
    }

    public class QRScanResponsPostModel 
    {
        public string? QRCode { get; set; }
    }

    public class QRScanResponseViewModel:APIBaseResponse
    {
        public int Type { get; set; }
        public int VisitID { get; set; }
        public CheckInCheckoutViewModel Meterial { get; set; }
        public VisitorCheckInCheckoutViewModel Visitor { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }
    }

    public class CheckinPostModel
    {
        public int VisitType { get; set; }
        public int VisitID { get; set; }
    }

    public class MyProfileInfoModel
    {
        public string? Name { get; set; }
        public string? QRCode { get; set; }
        public string? ProfileImage { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public string? UserTypeName { get; set; }
        public string? CompanyName { get; set; }
    }

    public class AddVehiclePostModel
    {
        public string? RegisterNo { get; set; }
    }

    public class AddCoPassendgerPostModel
    {
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        //[MaxLength(10)]
        //[RegularExpression(@"^05\d{5,10}$", ErrorMessage = "Please Enter Valid UAE Mobile Number (Eg: 0501234567 )")]
        public string? ContactNumber { get; set; }


        public List<DocumentUploadModel> Documents { get; set; }
    }

    public class DocumentUploadModel
    {
        [Range(1,int.MaxValue)]
        public int DocumentType { get; set; }
        public string? DocumentNo { get; set; }
        public DateTime? DocumentExpiry { get; set; }
        //public FileUploadModel DocumentImage { get; set; }
        public int MediaID { get; set; }
        public int MediaID2 { get; set; }
    }

    
    public class AddVehicleViewModel : APIBaseResponse
    {
        public List<ViVehicle> Vehicles { get; set; }
        public int? NewVehicleID { get; set; }
    }

    public class AddPassendgerViewModel : APIBaseResponse
    {
        public IEnumerable<IdnValuePair> Passengers { get; set; }
        public int? NewPassengerID { get; set; }
    }


    public class LocationModel
    {
        public int? LocationID { get; set; }
        public string? LocationName { get; set; }
        public int? LocationTypeID { get; set; }
    }


    public class RegistrationInitialDataModel : APIBaseResponse
    {
        public List<IdnValuePair> DocumentTypes { get; set; } = new List<IdnValuePair>();
    }

    public class SlotsViewModel:APIBaseResponse
    {
        public IEnumerable<IdnValuePair> Slots { get; set; }
    }

    public class RequestMeterialPostModel
    {
        public int RequestMeterialID { get; set; }
        public string? Description { get; set; }
        public string? Quantity { get; set; }
        public int? PackingTypeID { get; set; }
    }
}
