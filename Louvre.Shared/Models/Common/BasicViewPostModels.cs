using Microsoft.AspNetCore.Http;

namespace Louvre.Shared.Models
{

    public class IdnValueParentPair
    {
        public int ID { get; set; }
        public string? Value { get; set; }
        public int? ParentID { get; set; }
    }

    public class ImagesSaveViewModel
    {
        public bool IsSuccess { get; set; }
        public int? MediaID { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MediaSavePostViewModel
    {
        public int? MediaID { get; set; }
        public bool IsUrl { get; set; }
        public string? MediaURL { get; set; }
        public IFormFile MediaFile { get; set; }
        public string? FileURL { get; set; }
    }

    public class MediaFileOnlyPostViewModel
    {
        public int? MediaID { get; set; }
        public IFormFile MediaFile { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
