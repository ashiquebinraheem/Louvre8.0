using System;

namespace Louvre.Shared.Models
{
    public class DocumentPostViewModel
    {
        public int? DocumentID { get; set; }
        public int? DocumentTypeID { get; set; }
        public string? DocumentTypeName { get; set; }
        public int? MediaID { get; set; }
        public int? MediaID2 { get; set; }
        public bool HasFile { get; set; }
        public bool HasFile2 { get; set; }
        public string? FileName { get; set; }
        public string? FileName2 { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public int DocumentOf { get; set; }
        public bool IsRequired { get; set; }
    }
}
