using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public class RequestRejectMailModel
    {
        public string? Email { get; set; }
        public int RequestNo { get; set; }
        public string? Remarks { get; set; }
        public string? Slot { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
