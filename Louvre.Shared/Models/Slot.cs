using System;

namespace Louvre.Shared.Models
{
    public class SlotPostViewModel
    {
        public int? SlotID { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public int RequestCount { get; set; }
        public int RequestedCount { get; set; }
    }

    public class SlotListViewModel
    {
        public int SlotMasterID { get; set; }
        public string? Date { get; set; }
        public string? SlotGroupName { get; set; }
        public string? BranchName { get; set; }
    }

    public class CalenderViewModel
    {
        public string? start { get; set; }
        public string? end { get; set; }

        private string _title;
        public string? title
        {
            get
            {
                _title += " (" + RequestedCount.ToString() + '/' + RequestCount.ToString() + ")";
                return _title;
            }
            set { _title = value; }
        }

        private string _color;

        public string? color
        {
            get
            {
                int percent = (int)Math.Round((double)(100 * RequestedCount) / RequestCount);

                if (RequestedCount == 0)
                    _color = "green";
                else if (percent < 50)
                    _color = "#d4d404";
                else if (percent < 75)
                    _color = "orange";
                else
                    _color = "red";
                return _color;
            }
            set { _color = value; }
        }


        public int RequestCount { get; set; }
        public int RequestedCount { get; set; }

        public int groupId { get; set; } = 1;

        public int id { get; set; } = 1;

    }

    public class MonthSummaryViewModel
    {
        public int Day { get; set; }
        public int Requests { get; set; }
    }

    public class CalenderDetails
    {
        public int SlotID { get; set; }
        public string? EmployeeName { get; set; }
        public string? ModeName { get; set; }
        public string? LocationName { get; set; }
        public string? Slot { get; set; }


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
}
