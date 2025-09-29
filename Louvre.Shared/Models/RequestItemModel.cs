using Louvre.Shared.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public  class RequestItemModel: RequestItem
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? PurchaseUnit { get; set; }
        public string? IsExpirable { get; set; }
        public string? Type { get; set; }
        public string? Unit { get; set; }
    }
}
