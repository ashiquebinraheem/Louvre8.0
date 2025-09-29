using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public class DailyPassRequestApprovalHeaderView
    {
        public int DailyPassRequestID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? EmployeeName { get; set; }
        public string? DesignationName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public string? ModeName { get; set; }
        public string? RequestedLocationName { get; set; }
        public int RequestedByID { get; set; }
        public int RequestModeID { get; set; }
        public int BranchID { get; set; }
        public string? MeterialTypeName { get; set; }
        public bool ContainsExplosive { get; set; }
        public bool IsIn { get; set; }
        public bool IsDisposalRequired { get; set; }
        public int RequestNo { get; set; }
        //public string? HostEmail { get; set; }
        public string? Narration { get; set; }
        public string? Remarks { get; set; }
        public int MeterialTypeID { get; set; }

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
            get
            {
                switch (StatusID)
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

    }
}
