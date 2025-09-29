using Louvre.Shared.Core;
using System;

namespace Louvre.Shared.Models
{
    public class RequestListViewModel
    {
        public int RequestID { get; set; }
        public int RequestNo { get; set; }
        public string? Date { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public string? RequestedSlot { get; set; }
        public string? ModeName { get; set; }
        public string? RequestedLocationName { get; set; }
        public string? RequestedBy { get; set; }

        private string _Slot;
        public string? Slot
        {
            get
            {
                switch (StatusID)
                {
                    case (int)RequestStatus.Pending:
                    case (int)RequestStatus.Processing:
                    case (int)RequestStatus.Rejected:
                        _Slot = "";
                        break;
                }
                return _Slot;
            }
            set { _Slot = value; }
        }

        private string _LocationName;

        public string? LocationName
        {
            get
            {
                switch (StatusID)
                {
                    case (int)RequestStatus.Pending:
                    case (int)RequestStatus.Processing:
                    case (int)RequestStatus.Rejected:
                        _LocationName = "";
                        break;
                }
                return _LocationName;
            }
            set { _LocationName = value; }
        }

        public string? QRCode { get; set; }



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
        public string? Remarks { get; set; }
        public string? RequestTypeName { get; set; }
    }

    public class RequiestView
    {
        public int RequestID { get; set; }
        public DateTime Date { get; set; }
        public DateTime RequestedDate { get; set; }
        public int EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public int BranchID { get; set; }
        public string? BranchName { get; set; }
        public int SubBranchID { get; set; }
        public string? SubBranchName { get; set; }
        public int RequestedSlotID { get; set; }
        public string? RequestedSlot { get; set; }
        public int RequestModeID { get; set; }
        public string? ModeName { get; set; }
        public int RequestedLocationID { get; set; }
        public string? RequestedLocationName { get; set; }
        public int RequestedByID { get; set; }
        public string? RequestedBy { get; set; }
        public DateTime RequestedOn { get; set; }
        public bool IsRejected { get; set; }
        public int SlotID { get; set; }
        public string? Slot { get; set; }
        public int LocationID { get; set; }
        public string? LocationName { get; set; }
        public int ApprovalStage { get; set; }
        public bool NeedHigherLevelApproval { get; set; }
        public int StatusID { get; set; }
        public int? StorageLocationID { get; set; }
    }

    public class RequestApprovalHeaderView
    {
        public int RequestID { get; set; }
        public DateTime RequestedDate { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public string? RequestedSlot { get; set; }
        public string? ModeName { get; set; }
        public string? RequestedLocationName { get; set; }
        public int RequestedByID { get; set; }
        public int RequestModeID { get; set; }
        public int BranchID { get; set; }
        public int SlotID { get; set; }
        public string? MeterialTypeName { get; set; }
        public bool ContainsExplosive { get; set; }
        public bool ContainsCarryItem { get; set; }
        public bool IsIn { get; set; }
        public bool IsDisposalRequired { get; set; }
        public int RequestNo { get; set; }
        public string? HostEmail { get; set; }
        public string? Narration { get; set; }
        public string? Remarks { get; set; }
        public int MeterialTypeID { get; set; }
        public int StorageLocationID { get; set; }
        public string? LocationName { get; set; }


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


        private string _color;

        public string? Color
        {
            get { 
                switch(StatusID)
                {
                    case (int)RequestStatus.Accepted:
                        _color = "green";
                        break;
                    case (int)RequestStatus.Processing:
                        _color = "yellow";
                        break;
                    case (int)RequestStatus.Rejected:
                        _color = "red";
                        break;
                    case (int)RequestStatus.Pending:
                        _color = "orange";
                        break;
                }
                return _color; 
            }
        }

        public bool IsProjectAsset { get; set; }
        public string? PONumber { get; set; }
        public DateTime? PODate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int? POOwnerID { get; set; }

    }

    public class GatePassViewModel
    {
        public int RequestID { get; set; }
        public int RequestVehicleID { get; set; }
        public string? QRCode { get; set; }
        public string? CompanyName { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? ContactNumber { get; set; }
        public string? PlateNo { get; set; }
        public string? VehicleTypeName { get; set; }
        public string? VehicleSize { get; set; }
        public string? RegisterNo { get; set; }
        public string? Slot { get; set; }
        public string? LocationName { get; set; }
        public string? Email { get; set; }
        public string? GoogleLocation { get; set; }

        public int DailyPassRequestID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }


    public class RequestListPopupViewModel
    {
        public int RequestID { get; set; }
        public int RequestNo { get; set; }
        public string? Date { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public string? ModeName { get; set; }
        public string? RequestedBy { get; set; }
        public string? Slot { get; set; }
        public string? LocationName { get; set; }

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
        public string? Remarks { get; set; }
        public int RequestTypeID { get; set; }

        public string? ToDate { get; set; }
        public string? QRCode { get; set; }
        public int EmployeeID { get; set; }

    }

    public class EmployeeQRCodeModel
    {
        public int EmployeeID { get; set; }
        public string? QRCode { get; set; }
    }

    public class CheckinRequestListPopupViewModel: RequestListPopupViewModel
    {
        public int Type { get; set; }
        private string _TypeName;

        public string? TypeName
        {
            get {
                if (Type == 1)
                    _TypeName = "Meterial";
                else
                    _TypeName = "Scheduled Pass";

                return _TypeName; 
            }
        }

    }


    public class RequestForwardListViewModel
    {
        public int RequestNo { get; set; }
        public int RequestID { get; set; }
        public string? Date { get; set; }
        public string? EmployeeName { get; set; }
        public string? Remarks { get; set; }
        public string? CompanyName { get; set; }
    }

    public class RequestApprovalViewModel
    {
        public int? RequestApprovalID { get; set; }
        public int? RequestID { get; set; }
        public int? StorageLocationID { get; set; }
        public bool IsRejected { get; set; }
        public bool IsReported { get; set; }
        public string? Remarks { get; set; }
        public int? ApprovalStage { get; set; }
        public DateTime? Date { get; set; }
        public int? SlotID { get; set; }
        public int? LocationID { get; set; }
        public bool NeedHigherLevelApproval { get; set; }
        public int StatusID { get; set; }
    }

    public class RequestApprovalHistoryViewModel
    {
        public int Stage { get; set; }
        public string? UserGroup { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ProcessedOn { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class VehicleTrackingHistoryViewModel
    {
        public string? ProcessedBy { get; set; }
        public string? Status { get; set; }
        public string? AddedOn { get; set; }
    }

    public class RequestApproverMailModel
    {
        public int RequestID { get; set; }
        public string? Slot { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public string? RequestTypeName { get; set; }
        public string? EmailAddress { get; set; }
    }
}
