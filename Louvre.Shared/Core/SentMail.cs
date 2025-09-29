using Progbiz.DapperEntity;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Louvre.Shared.Core
{
    public class SentMail: BaseEntity
    {
        [PrimaryKey]
        public int? SentMailID { get; set; }
        public string? EmailAddress { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public DateTime? SentOn { get; set; }
        public bool HasSent { get; set; }
        public string? ErrorLog { get; set; }
    }
}
