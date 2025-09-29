using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Louvre.Shared.Models
{
	public class MeterialFileViewModel
	{
		public int MeterialMediaID { get; set; }
		public string? FileName { get; set; }

        public IFormFile File { get; set; }
        public int? MediaID { get; set; }
        public int RequestID { get; set; }

    }
}
