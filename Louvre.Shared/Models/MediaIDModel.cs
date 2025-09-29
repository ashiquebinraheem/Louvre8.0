using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Louvre.Shared.Models
{
    public class MediaIDModel
    {
        [Range(1,int.MaxValue)]
        public int MediaID { get; set; }
    }

    public class FileNameModel
    {
        public string? FileName { get; set; }
    }
}
