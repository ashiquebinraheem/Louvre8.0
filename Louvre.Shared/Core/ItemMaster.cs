using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class ItemMaster:BaseEntity
    {
        [PrimaryKey]
        public int ItemID { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? PurchaseUnit { get; set; }
        public bool IsExpirable { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
