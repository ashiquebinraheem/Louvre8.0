using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Core
{
    public class GeneralSettings : BaseEntity
    {
        [PrimaryKey]
        public int? SettingsKeyID { get; set; }
        public string? SettingsKey { get; set; }
        public string? SettingsValue { get; set; }
    }
}
