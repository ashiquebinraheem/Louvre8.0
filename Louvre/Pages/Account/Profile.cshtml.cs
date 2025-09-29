using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize]
    [BindProperties]
    public class ProfileModel : BasePageModel
    {
        private readonly IDbConnection cn;
        private readonly IDbContext _dbContext;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUserRepository _userRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public ProfileModel(IDbConnection cn, IDbContext dbContext, IMediaRepository mediaRepository, IUserRepository userRepository, IErrorLogRepository errorLogRepo)
        {
            this.cn = cn;
            _dbContext = dbContext;
            _mediaRepository = mediaRepository;
            _userRepository = userRepository;
            _errorLogRepo = errorLogRepo;
        }

        public User_Profile Data { get; set; }
        public PersonalInfo PersonalInfo { get; set; }
        public PersonalInfoAddress_Client Address { get; set; }
        public MediaFileOnlyPostViewModel Media { get; set; }

        public async Task OnGetAsync()
        {
            Data = await _dbContext.GetAsyncByFieldName<User_Profile>("PersonalInfoID", CurrentPersonalInfoID.ToString());
            PersonalInfo = await _dbContext.GetAsync<PersonalInfo>(CurrentPersonalInfoID);
            Address = await _dbContext.GetAsyncByFieldName<PersonalInfoAddress_Client>("PersonalInfoID", CurrentPersonalInfoID.ToString());
            Media = await _mediaRepository.GetMediaFileOnly(PersonalInfo.ProfileImageMediaID);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    Data.PersonalInfoID = PersonalInfo.PersonalInfoID;
                    await _dbContext.SaveAsync(Data, tran);

                    PersonalInfo.Email1 = Data.EmailAddress;
                    PersonalInfo.Phone1 = Data.MobileNumber;
                    PersonalInfo.PersonalInfoID = await _dbContext.SaveAsync(PersonalInfo, tran);
                    Address.PersonalInfoID = PersonalInfo.PersonalInfoID;
                    await _dbContext.SaveAsync(Address, tran);
                    await _mediaRepository.SaveProfilePic(Convert.ToInt32(PersonalInfo.PersonalInfoID), Media.MediaFile, Media.MediaID, tran);
                    var user = await _userRepository.GetCurrentClaim(CurrentUserID, tran);
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
                    identity.AddClaim(new Claim("UserID", user.UserID.ToString()));
                    identity.AddClaim(new Claim("UserTypeID", user.UserTypeID.ToString()));
                    identity.AddClaim(new Claim("ProfileURL", user.ProfileImageFileName.ToString()));
                    identity.AddClaim(new Claim("PersonalInfoID", user.PersonalInfoID.ToString()));
                    identity.AddClaim(new Claim("EmailAddress", user.EmailAddress));
                    identity.AddClaim(new Claim("MobileNumber", user.MobileNumber));
                    identity.AddClaim(new Claim("UserTypeName", user.UserTypeName));
                    identity.AddClaim(new Claim("UserStage", user.UserStage.ToString()));
                    string[] roles = (await _userRepository.GetUserRoles(user.UserID, tran)).ToArray();
                    foreach (var role in roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    var principal = new ClaimsPrincipal(identity);
                    var props = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();

                    tran.Commit();
                    result.CreatSuccessResponse(4);
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    result= await _errorLogRepo.CreatThrowResponse(err.Message,CurrentUserID);
                }
                return new JsonResult(result);
            }
        }
    }
}