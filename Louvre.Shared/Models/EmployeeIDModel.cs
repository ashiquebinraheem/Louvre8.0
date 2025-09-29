using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Louvre.Shared.Models
{
    public class EmployeeIDModel
    {
        [Range(1, int.MaxValue)]
        public int EmployeeID { get; set; }
    }
}
