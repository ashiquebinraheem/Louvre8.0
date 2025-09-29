using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Louvre.Shared.Models
{
    [Serializable]
    public class PreDefinedException : Exception
    {
        public BaseResponse Response { get; set; }

        public PreDefinedException(string message = "Error", string title = "", string description = "")
        {
            Response = new BaseResponse() { ResponseCode = -1000, ResponseMessage = message, ResponseTitle = title, ResponseErrorDescription = description };
        }
    }
}
