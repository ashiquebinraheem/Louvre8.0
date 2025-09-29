using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{

    public class DashboardDataPostModel
    {
        public int Period { get; set; }
    }

    public class DashboardDataModel
    {
        public SSFDashboardModel SSFDashboard { get; set; }
    }

    public class SSFDashboardModel
    {
        public int TotalRequester { get; set; }
        public int NotArrived { get; set; }
        public int CheckIn { get; set; }
        public int CheckOut { get; set; }
        public int DailyVisit { get; set; }
        public int LiveCapacity { get; set; }
    }

    public  class ChartDataModel
    {
        public List<string> Days { get; set; } = new List<string>();
        public List<int> Requests { get; set; } = new List<int>();
    }
}
