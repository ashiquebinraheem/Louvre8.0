using Progbiz.DapperEntity;
using System;
namespace Louvre.Shared.Core
{
    public class Document : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? DocumentID { get; set; }
        public int? EmployeeID { get; set; }
        public int? VehicleID { get; set; }
        public int? RequestID { get; set; }
        public int? CompanyID { get; set; }
        public int? VisitRequestID { get; set; }
        public int? DocumentTypeID { get; set; }
        public int? MediaID { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public int? MediaID2 { get; set; }
        public int? DailyPassRequestID { get; set; }
    }
}
