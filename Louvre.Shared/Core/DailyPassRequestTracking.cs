using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class DailyPassRequestTracking:BaseEntity
    {
        [PrimaryKey]
        public int TrackingID { get; set; }
        public int? DailyPassRequestID { get; set; }
        public bool IsCheckOut { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
