namespace Louvre.Shared.Models
{
    public class EmployeeListViewModel
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
    }




    public class EmployeeCalenderViewModel
    {
        public string? start { get; set; }
        public string? end { get; set; }
        public string? title { get; set; }

        private string _color;

        public string? color
        {
            get
            {
                switch (StatusID)
                {
                    case 1:
                        _color = "orange";
                        break;
                    case 3:
                        _color = "red";
                        break;
                    case 4:
                        _color = "green";
                        break;
                }
                return _color;
            }
            set { _color = value; }
        }


        public int StatusID { get; set; }
        public int id { get; set; }
        public int groupId { get; set; } = 2;
    }

    public class EmployeeCalenderDetails
    {
        public int VisitRequestID { get; set; }
        public string? PurposeName { get; set; }
        public string? Remark { get; set; }
    }
}
