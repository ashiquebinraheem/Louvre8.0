using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Louvre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected virtual int CurrentUserID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserID").Value); } }
        protected virtual int CurrentUserTypeID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserTypeID").Value); } }
        protected virtual int CurrentPersonalInfoID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "PersonalInfoID").Value); } }

        protected DateTime CurrentClientTime = DateTime.UtcNow.Date.AddMinutes(240);
    }
}