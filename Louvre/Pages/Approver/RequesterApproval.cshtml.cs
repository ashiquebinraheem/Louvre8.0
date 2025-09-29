//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Louvre.Shared.Core;
//using Progbiz.DapperEntity;
//using Louvre.Shared.Models;
//using Louvre.Shared.Repository;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Louvre.Pages
//{
//    [BindProperties]
//    [Authorize(Roles = "Super-Admin, Administrator, Approver,Disposal")]
//    public class RequesterApprovalModel : BasePageModel
//    {
//        private readonly IDbConnection cn;
//        private readonly IDbContext _dbContext;
//        private readonly IUserRepository _userRepository;
//        private readonly ICommonRepository _commonRepository;
//        private readonly IMediaRepository _mediaRepository;
//        private readonly IEmailSender _emailSender;
//        private readonly IErrorLogRepository _errorLogRepo;

//        public RequesterApprovalModel(IDbConnection cn, IDbContext dbContext, IUserRepository userRepository, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IErrorLogRepository errorLogRepo)
//        {
//            this.cn = cn;
//            _dbContext = dbContext;
//            _userRepository = userRepository;
//            _commonRepository = commonRepository;
//            _mediaRepository = mediaRepository;
//            _emailSender = emailSender;
//            _errorLogRepo = errorLogRepo;
//        }

//        public int UserTypeID { get; set; }
//        public Core.Company Company { get; set; }
//        public User UserData { get; set; }
//        public PersonalInfo_Client PersonalInfo { get; set; }
//        public PersonalInfoAddress_Client Address { get; set; }

//        public List<DocumentPostViewModel> Documents { get; set; }
//        public UserModule UserModule { get; set; }

//        public async Task OnGetAsync(int id)
//        {
//            ViewData["Modules"] = new SelectList(await _dbContext.GetAllAsync<Module>(), "ModuleID", "ModuleName");
//            UserData = await _dbContext.GetAsync<User>(id);
//            PersonalInfo = await _dbContext.GetAsync<PersonalInfo_Client>(Convert.ToInt32(UserData.PersonalInfoID));
//            UserModule = await _dbContext.GetAsyncByFieldName<UserModule>("UserID", id.ToString());
//            Documents = new List<DocumentPostViewModel>();
//            Company = await _dbContext.GetAsyncByFieldName<Core.Company>("PersonalInfoID", Convert.ToInt32(UserData.PersonalInfoID).ToString());

//            if (UserData.UserTypeID == (int)UserTypes.Company)
//            {
//                if (Company != null)
//                    Documents = (await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, Convert.ToInt32(Company.CompanyID))).Where(s => !string.IsNullOrEmpty(s.FileName)).ToList();

//            }
//            else if (UserData.UserTypeID == (int)UserTypes.Individual)
//            {
//                var employee = await _dbContext.GetAsyncByFieldName<Core.Employee>("PersonalInfoID", Convert.ToInt32(UserData.PersonalInfoID).ToString());
//                if (employee != null)
//                    Documents = (await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, Convert.ToInt32(employee.EmployeeID))).Where(s => !string.IsNullOrEmpty(s.FileName)).ToList();
//            }
//            else
//            {
//                UserData = null;
//                PersonalInfo = null;
//            }
//        }

//        public async Task<IActionResult> OnPostAcceptAsync()
//        {
//            BaseResponse result = new BaseResponse();

//            try
//            {
//                await _dbContext.ExecuteAsync($@"Update Users set IsApproved=1,ApprovedBy={CurrentUserID},ApprovedOn=@ApprovedOn where UserID={UserData.UserID}", new { ApprovedOn = DateTime.UtcNow });
//                UserModule.UserID = UserData.UserID;
//                await _dbContext.SaveAsync(UserModule);

//                var personalInfo = await _dbContext.GetAsync<ViPersonalInfo>($"Select Name,Email1 From viPersonalInfos where UserID=@UserID", new {UserID= UserData.UserID });

//                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

//                var body = $@"
//						Dear {personalInfo.Name},<br>
//						<h3>Your account is approved</h3>
//						Now you can send request to us.Please Login through	<a href='{url}/login'>here</a>";
//                await _emailSender.SendHtmlEmailAsync(personalInfo.Email1, "Account approved", body);

//                result.CreatSuccessResponse(105);
//            }
//            catch (Exception err)
//            {
//                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
//            }
//            return new JsonResult(result);
//        }

//        public async Task<IActionResult> OnPostRejectAsync()
//        {
//            BaseResponse result = new BaseResponse();

//            try
//            {
//                await _dbContext.ExecuteAsync($@"Update Users set IsRejected=1,ApprovedBy={CurrentUserID},ApprovedOn=@ApprovedOn where UserID={UserData.UserID}", new { ApprovedOn = DateTime.UtcNow });
//                result.CreatSuccessResponse(106);
//            }
//            catch (Exception err)
//            {
//                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
//            }
//            return new JsonResult(result);
//        }
//    }
//}

using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [BindProperties]
    [Authorize(Roles = "Super-Admin, Administrator, Approver, Disposal")]
    public class RequesterApprovalModel : BasePageModel
    {
        private readonly IDbConnection _cn;
        private readonly IDbContext _dbContext;
        private readonly IUserRepository _userRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IErrorLogRepository _errorLogRepo;

        public RequesterApprovalModel(IDbConnection cn, IDbContext dbContext, IUserRepository userRepository, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IErrorLogRepository errorLogRepo)
        {
            _cn = cn;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _errorLogRepo = errorLogRepo;
        }

        public int UserTypeID { get; set; }
        public Louvre.Shared.Core.Company Company { get; set; }
        public User UserData { get; set; }
        public PersonalInfo_Client PersonalInfo { get; set; }
        public PersonalInfoAddress_Client Address { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }
        public UserModule UserModule { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            UserData = await _dbContext.GetAsync<User>(id);
            if (UserData == null)
                return NotFound();

            ViewData["Modules"] = new SelectList(await _dbContext.GetAllAsync<Module>(), "ModuleID", "ModuleName");

            PersonalInfo = await _dbContext.GetAsync<PersonalInfo_Client>(Convert.ToInt32(UserData.PersonalInfoID));
            UserModule = await _dbContext.GetAsyncByFieldName<UserModule>("UserID", id.ToString());
            Documents = new List<DocumentPostViewModel>();
            Company = await _dbContext.GetAsyncByFieldName<Louvre.Shared.Core.Company>("PersonalInfoID", Convert.ToInt32(UserData.PersonalInfoID).ToString());

            if (UserData.UserTypeID == (int)UserTypes.Company && Company != null)
            {
                Documents = (await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, Convert.ToInt32(Company.CompanyID)))
                            .Where(d => !string.IsNullOrEmpty(d.FileName))
                            .ToList();
            }
            else if (UserData.UserTypeID == (int)UserTypes.Individual)
            {
                var employee = await _dbContext.GetAsyncByFieldName<Louvre.Shared.Core.Employee>("PersonalInfoID", Convert.ToInt32(UserData.PersonalInfoID).ToString());
                if (employee != null)
                {
                    Documents = (await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, Convert.ToInt32(employee.EmployeeID)))
                                .Where(d => !string.IsNullOrEmpty(d.FileName))
                                .ToList();
                }
            }
            else
            {
                UserData = null;
                PersonalInfo = null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync()
        {
            var result = new BaseResponse();

            try
            {
                // Parameterized query for safety  
                //  modified by  Abdul Razack -- for 9: Information Disclosure via Verbose Error Messages and Path Disclosure
                await _dbContext.ExecuteAsync(
                    @"UPDATE Users
                      SET IsApproved = 1, ApprovedBy = @ApprovedBy, ApprovedOn = @ApprovedOn
                      WHERE UserID = @UserID",
                    new { ApprovedBy = CurrentUserID, ApprovedOn = DateTime.UtcNow, UserID = UserData.UserID }
                );

                UserModule.UserID = UserData.UserID;
                await _dbContext.SaveAsync(UserModule);

                var personalInfo = await _dbContext.GetAsync<ViPersonalInfo>(
                    "SELECT Name, Email1 FROM viPersonalInfos WHERE UserID = @UserID",
                    new { UserID = UserData.UserID }
                );

                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                // Encode Name to prevent HTML injection
                var safeName = HtmlEncoder.Default.Encode(personalInfo.Name);

                var body = $@"
                    Dear {safeName},<br>
                    <h3>Your account is approved</h3>
                    Now you can send requests to us. Please login through <a href='{url}/login'>here</a>.";

                await _emailSender.SendHtmlEmailAsync(personalInfo.Email1, "Account Approved", body);

                result.CreatSuccessResponse(105);
            }
            catch (Exception ex)
            {
                // Log full exception internally, but do not expose to client
                result = await _errorLogRepo.CreatThrowResponse(ex.Message, CurrentUserID);
            }

            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            var result = new BaseResponse();

            try
            {
                await _dbContext.ExecuteAsync(
                    @"UPDATE Users
                      SET IsRejected = 1, ApprovedBy = @ApprovedBy, ApprovedOn = @ApprovedOn
                      WHERE UserID = @UserID",
                    new { ApprovedBy = CurrentUserID, ApprovedOn = DateTime.UtcNow, UserID = UserData.UserID }
                );

                result.CreatSuccessResponse(106);
            }
            catch (Exception ex)
            {
                result = await _errorLogRepo.CreatThrowResponse(ex.Message, CurrentUserID);
            }

            return new JsonResult(result);
        }
    }
}


