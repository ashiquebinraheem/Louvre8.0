using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class RequestItem:AuditableBaseEntity
    {
        [PrimaryKey]
        public int? RequestItemID { get; set; }
        public int? RequestID { get; set; }
        public int? ItemID { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Value { get; set; }
        public string? OEM { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNo { get; set; }
        public DateTime? WarrantyDate { get; set; }
        public string? ManufactureYear { get; set; }
        public string? ExpectedLifeYear { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
