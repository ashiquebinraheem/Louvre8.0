using Progbiz.DapperEntity;

namespace Louvre.Shared.Core
{
    public class DocumentType : AuditableBaseEntity
    {
        [PrimaryKey]
        public int? DocumentTypeID { get; set; }
        public string? DocumentTypeName { get; set; }
        public int? DocumentTypeCategoryID { get; set; }
        public bool IsRequired { get; set; }
    }
}
