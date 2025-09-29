using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    [BindProperties]
    public class EmployeeModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public EmployeeModel(IDbContext dbContext, IDbConnection cn, IUserRepository userRepository, IMediaRepository mediaRepository, IErrorLogRepository errorLogRepo)
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

        public string? NewPassword { get; set; }

        public async Task OnGetAsync(int? id)
        {
            int? mediaId = null;
            if (id != null)
            {
                Data = await _dbContext.GetAsync<User>(Convert.ToInt32(id));
                PersonalInfo = await _dbContext.GetAsync<PersonalInfo_Client>(Convert.ToInt32(Data.PersonalInfoID));
                mediaId = PersonalInfo.ProfileImageMediaID;
            }
            Media = await _mediaRepository.GetMediaFileOnly(mediaId);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            Data.UserName = PersonalInfo.Email1;
            Data.UserTypeID = (int)UserTypes.Employee;

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
                Data.Salt = exData.Salt;
                Data.Password = exData.Password;
            }

            var isExist = await _userRepository.CheckExist(Convert.ToInt32(Data.UserID), Data.UserName);
            if (isExist)
            {
                var response = new BaseResponse(-8);
                return new JsonResult(response);
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