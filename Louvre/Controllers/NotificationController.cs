using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Louvre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public NotificationController(IDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [Route("get-notification")]
        [HttpGet]
        public async Task<List<NotificationViewModel>> GetNotifications()
        {
            List<NotificationViewModel> notifications = new List<NotificationViewModel>();
            if(User.IsInRole("Super-Admin") || User.IsInRole("Administrator") || User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                
                var cnt = await _dbContext.GetAsync<int>($@"SELECT  Count(UserID)
                FROM Users U
                LEFT JOIN PersonalInfos P on P.PersonalInfoID=U.PersonalInfoID
			    Where ISNULL(U.IsDeleted,0)=0 and EmailConfirmed=1 and ISNULL(IsApproved,0)=0 and 
                ISNULL(IsRejected,0)=0 and UserTypeID in({(int)UserTypes.Company},{(int)UserTypes.Individual})",
                null);

                if (cnt > 0)
                    notifications.Add(new NotificationViewModel() { RedirectURL = "new-requesters", Icon = "fa fa-user-plus", Notification = $"{cnt} new login requests for approval" });
            }

            if (User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                var cnt=await _dbContext.GetAsync<int>($@"SELECT  Count(R.RequestID)
                FROM  viRequest R
			    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID and S.Stage=ISNULL(R.ApprovalStage,0)+1
			    JOIN UserTypes U on U.UserTypeID=S.UserTypeID
			    LEFT JOIN RequestTypeApprovalStage S1 on S1.RequestTypeID=R.RequestTypeID and S1.Stage=ISNULL(R.ApprovalStage,0)+2
			    Where IsRejected=0 and ISNULL(NeedHigherLevelApproval,1)=1 and CASE WHEN U.UserNature=2 and 
                R.IsDisposalRequired=0 then S1.UserTypeID else S.UserTypeID end=@CurrentUserTypeID  and Date>=@Date",
                new { CurrentUserTypeID,CurrentClientTime.Date });

                if (cnt > 0)
                    notifications.Add(new NotificationViewModel() { RedirectURL = "new-requests", Icon = "fas fa-shipping-fast", Notification = $"{cnt} new material requests for approval" });
            
                var cnt1 = await _dbContext.GetAsync<int>($@"SELECT  Count(R.DailyPassRequestID)
                FROM  viDailyPassRequest R
			    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID and S.Stage=ISNULL(R.ApprovalStage,0)+1
			    JOIN UserTypes U on U.UserTypeID=S.UserTypeID
			    LEFT JOIN RequestTypeApprovalStage S1 on S1.RequestTypeID=R.RequestTypeID and S1.Stage=ISNULL(R.ApprovalStage,0)+2
			    Where IsRejected=0 and ISNULL(NeedHigherLevelApproval,1)=1 and CASE WHEN U.UserNature=2 
                and R.IsDisposalRequired=0 then S1.UserTypeID else S.UserTypeID end=@CurrentUserTypeID  
                and FromDate>=@FromDate", new { FromDate = CurrentClientTime.Date, CurrentUserTypeID });

                if (cnt1 > 0)
                    notifications.Add(new NotificationViewModel() { RedirectURL = "new-daily-pass-requests", Icon = "fas fa-shipping-fast", Notification = $"{cnt1} new Scheduled Pass requests for approval" });
            }

            if (User.IsInRole("Security-Control-Room") || User.IsInRole("Security-Duty-Manager"))
            {
                var cnt = await _dbContext.GetAsync<int>($@"SELECT Count(R.RequestID)
                FROM  viRequest R
			    where StatusID={(int)RequestStatus.Forwarded}",null);

                if (cnt > 0)
                    notifications.Add(new NotificationViewModel() { RedirectURL = "new-forwards", Icon = "fas fa-forward", Notification = $"{cnt} new forward from security gate" });
            }

            if (User.IsInRole("Employee"))
            {
                var cnt = await _dbContext.GetAsync<int>($@"SELECT Count(R.VisitRequestID)
                FROM  viVisitRequest R
			    where StatusID={(int)RequestStatus.Pending} and HostUserID=@CurrentUserID", new { CurrentUserID });

                if (cnt > 0)
                    notifications.Add(new NotificationViewModel() { RedirectURL = "new-visit-requests", Icon = "fas fa-shipping-fast", Notification = $"{cnt} new visit request received" });
            }

            return notifications;
        }

        [HttpPost("save-file")]
        public IActionResult SaveFile(MediaServerPostModel model)
        {
            try
            {
                int size = 700;
                int width, height;

                if (!string.IsNullOrEmpty(model.DeleteFileName))
                {
                    string deleteFilePath = Path.Combine(_env.ContentRootPath, "wwwroot");
                    deleteFilePath += model.DeleteFileName;
                    System.IO.File.Delete(deleteFilePath);
                }

                string path = Path.Combine(_env.ContentRootPath, model.NewImageFileName);

                switch (model.ContentType.ToLower())
                {
                    case "image/jpeg":
                    case "image/jpg":
                    case "image/png":
                        var image = Image.Load(model.Content);

                        if (image.Width > image.Height)
                        {
                            if (image.Width < size)
                                size = image.Width;

                            width = size;
                            height = image.Height * size / image.Width;
                        }
                        else
                        {
                            if (image.Height < size)
                                size = image.Height;

                            width = image.Width * size / image.Height;
                            height = size;
                        }

                        image.Mutate(x => x.Resize(width, height));
                        image.Save(Path.Combine(path));
                        break;
                    default:
                        var fs = System.IO.File.Create(path);
                        fs.Write(model.Content, 0, model.Content.Length);
                        fs.Close();
                        break;
                }

                return Ok();
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }
    }
}
