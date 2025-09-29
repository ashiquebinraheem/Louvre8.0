using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Louvre.Shared.Models
{
    public class LoadingBayVerifyPostModel
    {
        [Required]
        public int? RequestID { get; set; }
    }
}
