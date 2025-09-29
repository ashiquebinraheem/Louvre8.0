using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public class DailyPassRequestListViewModel
    {
        public int DailyPassRequestID { get; set; }
        public int RequestNo { get; set; }
        public string? Date { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? EmployeeName { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? SubBranchName { get; set; }
        public string? RequestedSlot { get; set; }
        public string? ModeName { get; set; }
        public string? RequestedLocationName { get; set; }
        public string? RequestedBy { get; set; }

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

        public string? QRCode;
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
}
