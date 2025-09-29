using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Louvre.Shared.Models
{
    public class RequestIDModel
    {
        [Range(1,int.MaxValue)]
        public int RequestID { get; set; }
    }

    public class VisitRequestIDModel
    {
        [Range(1, int.MaxValue)]
        public int VisitRequestID { get; set; }
    }

    
}
