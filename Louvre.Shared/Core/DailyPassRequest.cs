using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class DailyPassRequest:AuditableBaseEntity
    {
        [PrimaryKey]
        public int? DailyPassRequestID { get; set; }
        public DateTime? Date { get; set; }
        public int RequestNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? EmployeeID { get; set; }
        public int? BranchID { get; set; }
        public int? SubBranchID { get; set; }
        public int? RequestModeID { get; set; }
        public int? LocationID { get; set; }
        public bool ContainsExplosive { get; set; }
        public bool IsDisposalRequired { get; set; }
        public int? MeterialTypeID { get; set; }
        //public string? HostEmail { get; set; }
        public string? Narration { get; set; }
        public bool IsLoadingBayVerified { get; set; }
        public int? VehicleID { get; set; }
        public int? DriverID { get; set; }
        public int PassengersCount { get; set; }
        public bool FromApp { get; set; }
    }
}
