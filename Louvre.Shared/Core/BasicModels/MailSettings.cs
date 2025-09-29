using Progbiz.DapperEntity;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    [TableName("MailSettings")]
    public class MailSettings : BaseEntity
    {
        [PrimaryKey]
        public int MailSettingsID { get; set; }
        public string? SMTPHost { get; set; }
        public int Port { get; set; }
        public string? FromName { get; set; }
        [EmailAddress]
        public string? FromMail { get; set; }
        public string? Password { get; set; }
        public bool EnableSSL { get; set; }
        public string? DefaultSubject { get; set; }
        public string? MailTo { get; set; }
        public string? MailBody { get; set; }
        //public string? WebBaseURL { get; set; }
    }
}
