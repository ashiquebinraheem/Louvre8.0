using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
    public class MediaServerPostModel: FileUploadModel
    {
        public string? DeleteFileName { get; set; }
        public string? NewImageFileName { get; set; }
    }
}
