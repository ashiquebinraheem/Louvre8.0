using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
	public class RequestMeterialMedia: AuditableBaseEntity
    {
        [PrimaryKey]
        public int? MeterialMediaID { get; set; }
        public int? RequestID { get; set; }
        public int? MediaID { get; set; }
        public int? DailyPassRequestID { get; set; }
    }
}
