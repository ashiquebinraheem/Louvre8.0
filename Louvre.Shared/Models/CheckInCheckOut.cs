using System;
using System.Collections.Generic;

namespace Louvre.Shared.Models
{
    public class CheckInCheckoutViewModel
    {
        public CheckInCheckoutViewModel()
        {
            Passengers = new List<RequesterPostViewModel>();
            Meterials = new List<MeterialViewModel>();

        }
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
        public int PassengerCount { get; set; }
        public string? Slot { get; set; }
        public string? LocationName { get; set; }
        public bool NeedCheckin { get; set; }
        public bool NeedCheckout { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public bool ContainsExplosive { get; set; }
        public string? RequestTypeName { get; set; }

        public List<RequesterPostViewModel> Passengers { get; set; }
        public List<MeterialViewModel> Meterials { get; set; }

        public DateTime? AllotedDate { get; set; }

        public bool NeedLoadingBayVerify { get; set; }
        public bool IsLoadingBayVerified { get; set; }
    }

    public class VisitorCheckInCheckoutViewModel
    {
        public int VisitRequestID { get; set; }
        public string? Requester { get; set; }
        public string? QRCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? AreaName { get; set; }
        public string? PurposeName { get; set; }
        public string? EmployeeName { get; set; }
        public string? MeetingDate { get; set; }
        public string? DurationName { get; set; }
        public string? Remark { get; set; }
        public string? PlateNo { get; set; }
        public string? RegisterNo { get; set; }
        public string? IsParkingRequired { get; set; }
        public bool NeedCheckin { get; set; }
        public bool NeedCheckout { get; set; }
        public string? ContactNumber { get; set; }

    }

    public class DailyPassCheckInCheckoutViewModel
    {
        public DailyPassCheckInCheckoutViewModel()
        {
            Passengers = new List<RequesterPostViewModel>();
            Meterials = new List<MeterialViewModel>();

        }
        public int DailyPassRequestID { get; set; }
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
        public int PassengerCount { get; set; }
        public string? LocationName { get; set; }
        public bool NeedCheckin { get; set; }
        public bool NeedCheckout { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public bool ContainsExplosive { get; set; }
        public string? RequestTypeName { get; set; }

        public List<RequesterPostViewModel> Passengers { get; set; }
        public List<MeterialViewModel> Meterials { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public bool NeedLoadingBayVerify { get; set; }
        public bool IsLoadingBayVerified { get; set; }
    }
}
