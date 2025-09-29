using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class ErrorLog: BaseEntity
    {
        [PrimaryKey]
        public int ErrorLogID { get; set; }
        public int? UserID { get; set; }
        public string? Error { get; set; }
        public DateTime? AddedOn { get; set; }
    }
}
