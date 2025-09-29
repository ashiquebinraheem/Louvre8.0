using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Louvre.Shared.Models
{
    public class VehicleIDModel
    {
        [Range(1,int.MaxValue)]
        public int VehicleID { get; set; }
    }
}
