using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Progbiz.DapperEntity;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages.PageModels
{
    public class BasePageModel : PageModel
    {
        protected virtual int CurrentUserID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserID").Value); } }

        protected virtual int CurrentUserTypeID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UserTypeID").Value); } }

        protected virtual int CurrentPersonalInfoID { get { return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "PersonalInfoID").Value); } }


        protected async Task<SelectList> GetSelectList<T>(IDbContext _dbContext, string valueFieldName, string whereCondition = "", IDbTransaction transaction = null) where T : BaseEntity
        {
            return new SelectList((await _dbContext.GetIdValuePairAsync<T>(valueFieldName, whereCondition, transaction)).ToList(), "ID", "Value");
        }

        protected DateTime GetClientTime(IHttpContextAccessor httpContextAccessor)
        {
            var timeStamp = httpContextAccessor.HttpContext.Request.Cookies["timezoneoffset"];
            if (timeStamp == null)
                timeStamp = "0";

            return DateTime.UtcNow.AddMinutes(Convert.ToInt16(timeStamp));
        }

        protected int GetClientTimeZone(IHttpContextAccessor httpContextAccessor)
        {
            return Convert.ToInt32(httpContextAccessor.HttpContext.Request.Cookies["timezoneoffset"]);
        }


        protected string SQLDateFormate = "yyyy-MM-dd HH:mm:ss.fff";


        protected DateTime CurrentClientTime = DateTime.UtcNow.Date.AddMinutes(240);
    }
}
