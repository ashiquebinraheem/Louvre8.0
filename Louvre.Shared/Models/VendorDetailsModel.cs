using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public  class VendorDetailsModel
    {
        public int CompanyID { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPersonNumber { get; set; }
        public string? CompanyAddress { get; set; }
        public int VendorID { get; set; }
    }
}
