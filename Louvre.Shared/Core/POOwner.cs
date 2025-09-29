using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class POOwner:BaseEntity
    {
        [PrimaryKey]
        public int POOwnerID { get; set; }
        public string? StaffName { get; set; }
        public string? Designation { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
