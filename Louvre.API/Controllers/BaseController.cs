using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Progbiz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected virtual int CurrentUserID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserID").Value); } }
        protected virtual int CurrentUserTypeID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserTypeID").Value); } }
        protected virtual int CurrentPersonalInfoID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "PersonalInfoID").Value); } }
        protected int TimeOffset = 240;
        protected DateTime CurrentClientTime = DateTime.UtcNow.Date.AddMinutes(240);
    }
}
