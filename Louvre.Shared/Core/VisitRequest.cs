using Progbiz.DapperEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    public class VisitRequest : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? VisitRequestID { get; set; }
        [Required(ErrorMessage = "Select Requester")]
        public int? EmployeeID { get; set; }
        [Required(ErrorMessage = "Select Department")]
        public int? DepartmentID { get; set; }
        [Required(ErrorMessage = "Select Area")]
        public int? AreaID { get; set; }
        [Required(ErrorMessage = "Select Purpose")]
        public int? PurposeID { get; set; }
        public int? HostUserID { get; set; }
        [Required(ErrorMessage = "Enter Meeting Date")]
        public DateTime? MeetingDate { get; set; }
        [Required(ErrorMessage = "Select Duration")]
        public int? DuraionID { get; set; }
        public string? Remark { get; set; }
        [Required(ErrorMessage = "Select Vehicle")]
        public int? VehicleID { get; set; }
        public bool IsParkingRequired { get; set; }
        //public string? QRCode { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public string? RejectReason { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}
