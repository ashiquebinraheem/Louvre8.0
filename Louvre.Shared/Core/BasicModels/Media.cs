using Progbiz.DapperEntity;
using System.ComponentModel.DataAnnotations;

namespace Louvre.Shared.Core
{
    [TableName("Medias")]
    public class Media : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? MediaID { get; set; }
        public bool IsURL { get; set; }
        [Required]
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public string? ContentType { get; set; }
        public long ContentLength { get; set; }
    }
}
