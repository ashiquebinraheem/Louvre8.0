using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin,Administrator")]
    [BindProperties]
    public class UserModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public UserModel(IDbContext dbContext, IDbConnection cn, IUserRepository userRepository, IMediaRepository mediaRepository, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _errorLogRepo = errorLogRepo;
        }

        public User Data { get; set; }
        public PersonalInfo_Client PersonalInfo { get; set; }
        public MediaFileOnlyPostViewModel Media { get; set; }


        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string? NewPassword { get; set; }
        public async Task OnGetAsync(int? id)
        {
            var userType = await _dbContext.GetAsync<UserType>(CurrentUserTypeID);
            int? mediaId = null;
            int? userTypeId = null;
            int? branchId = null;
            if (id != null)
            {


                Data = await _dbContext.GetAsync<User>(Convert.ToInt32(id));
                var currentUserType = await _dbContext.GetAsync<UserType>(Data.UserTypeID);

                if (currentUserType.PriorityOrder <= userType.PriorityOrder)
                {
                    Data = null;
                }
                else
                {
                    Data.Salt = string.Empty;

                    PersonalInfo = await _dbContext.GetAsync<PersonalInfo_Client>(Convert.ToInt32(Data.PersonalInfoID));
                    mediaId = PersonalInfo.ProfileImageMediaID;
                    userTypeId = Data.UserTypeID;
                }
            }
            Media = await _mediaRepository.GetMediaFileOnly(mediaId);
            ViewData["UserTypes"] = new SelectList((await _dbContext.GetAllAsync<UserType>()).ToList().Where(t => t.PriorityOrder > userType.PriorityOrder && t.ShowInList == true), "UserTypeID", "UserTypeName", userTypeId);
            ViewData["Branches"] = new SelectList((await _dbContext.GetAllAsync<Branch>()).ToList(), "BranchID", "BranchName", branchId);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new JsonResult(new BaseResponse
                {
                    ResponseCode = -1,
                    ResponseTitle = "Validation Error",
                    ResponseMessage = string.Join("\n", errors)
                });
            }

            var userType = await _dbContext.GetAsync<UserType>(CurrentUserTypeID);
            var userTypes = (await _dbContext.GetAllAsync<UserType>()).ToList().Where(t => t.PriorityOrder > userType.PriorityOrder && t.ShowInList == true);
            bool isFound = false;
            foreach(var ut in userTypes)
            {
                if(Data.UserTypeID==ut.UserTypeID)
                {
                    isFound = true;
                    break;
                }
            }

            if(!isFound)
            {
                var response = new BaseResponse(-112);
                return new JsonResult(response);
            }

            var isExist = await _userRepository.CheckExist(Convert.ToInt32(Data.UserID), Data.UserName);
            if (isExist)
            {
                var response = new BaseResponse(-8);
                return new JsonResult(response);
            }

            if (!string.IsNullOrEmpty(NewPassword))
            {
                Data.Salt = Guid.NewGuid().ToString("n").Substring(0, 8);
                Data.Password = UserRepository.GetHashPassword(NewPassword, Data.Salt);
            }
            
            if (Data.UserID == null)
            {
                Data.Salt = Guid.NewGuid().ToString("n").Substring(0, 8);
                Data.Password = UserRepository.GetHashPassword(Data.Password, Data.Salt);
            }
            else if (string.IsNullOrEmpty(NewPassword))
            {
                User exData = await _dbContext.GetAsync<User>(Convert.ToInt32(Data.UserID));
                Data.Salt= exData.Salt;
                Data.Password= exData.Password;
            }

            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    PersonalInfo.PersonalInfoID = await _dbContext.SaveAsync(PersonalInfo, tran);
                    await _mediaRepository.SaveProfilePic(Convert.ToInt32(PersonalInfo.PersonalInfoID), Media.MediaFile, Media.MediaID, tran);
                    Data.PersonalInfoID = PersonalInfo.PersonalInfoID;
                    Data.EmailAddress = PersonalInfo.Email1;
                    Data.MobileNumber = PersonalInfo.Phone1;
                    Data.EmailConfirmed = true;
                    await _dbContext.SaveAsync(Data, tran);
                    tran.Commit();
                    result.CreatSuccessResponse(1);
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                }

                return new JsonResult(result);
            }
        }
    }
}