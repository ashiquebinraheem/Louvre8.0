using System;

namespace Louvre.Shared.Models
{
    public class VisitRequestListViewModel
    {
        public int VisitRequestID { get; set; }
        public string? Requester { get; set; }
        public string? DepartmentName { get; set; }
        public string? EmployeeName { get; set; }
        public string? AreaName { get; set; }
        public string? PurposeName { get; set; }
        public string? MeetingDate { get; set; }
        public string? DurationName { get; set; }
        public string? Remark { get; set; }

        public int StatusID { get; set; }

        private string _Status;
        public string? Status
        {
            get
            {
                var enumDisplay = (RequestStatus)StatusID;
                _Status = enumDisplay.ToString();
                return _Status;
            }
        }
    }

    public class VisitRequestView
    {
        public int? VisitRequestID { get; set; }
        public int? EmployeeID { get; set; }
        public string? Requester { get; set; }
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public int? AreaID { get; set; }
        public string? AreaName { get; set; }
        public int? PurposeID { get; set; }
        public string? PurposeName { get; set; }
        public int? HostUserID { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeEmailAddress { get; set; }
        public DateTime? MeetingDate { get; set; }
        public int? DuraionID { get; set; }
        public string? DurationName { get; set; }
        public string? Remark { get; set; }
        public int? VehicleID { get; set; }
        public string? PlateNo { get; set; }
        public string? RegisterNo { get; set; }
        public bool IsParkingRequired { get; set; }
        public string? QRCode { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public int StatusID { get; set; }
        public string? RequesterEmail { get; set; }
    }


    public class VisitRequestListPopupViewModel
    {
        public int VisitRequestID { get; set; }
        public string? Requester { get; set; }
        public string? CompanyName { get; set; }
        public string? PurposeName { get; set; }
        public string? MeetingDate { get; set; }
        public string? DurationName { get; set; }
        public int StatusID { get; set; }

        private string _Status;
        public string? Status
        {
            get
            {
                var enumDisplay = (RequestStatus)StatusID;
                _Status = enumDisplay.ToString();
                return _Status;
            }
        }
        public string? RequestTypeName { get; set; }

    }

}
