using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class RequestStorageLocationType:BaseEntity
    {
        [PrimaryKey]
        public int? MeterialStorageLocationTypeID { get; set; }
        public string? MeterialStorageLocationTypeName { get; set; }
    }
}
