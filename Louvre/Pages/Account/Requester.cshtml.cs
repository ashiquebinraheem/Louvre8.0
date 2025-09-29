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
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [BindProperties]
    [Authorize(Roles = "Super-Admin, Administrator, Approver,Disposal")]
    public class RequesterModel : BasePageModel
    {
        private readonly IDbConnection cn;
        private readonly IDbContext _dbContext;
        private readonly IUserRepository _userRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public RequesterModel(IDbConnection cn, IDbContext dbContext, IUserRepository userRepository, ICommonRepository commonRepository, IErrorLogRepository errorLogRepo)
        {
            this.cn = cn;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _commonRepository = commonRepository;
            _errorLogRepo = errorLogRepo;
        }

        public int UserTypeID { get; set; }
        public Louvre.Shared.Core.Company Company { get; set; }
        public User Data { get; set; }
        public PersonalInfo_Client PersonalInfo { get; set; }
        public PersonalInfoAddress_Client Address { get; set; }

        public List<DocumentPostViewModel> Documents { get; set; }
        public List<UserModule> UserModules { get; set; }


        public async Task OnGetAsync(int id)
        {
            ViewData["Modules"] = new SelectList(await _dbContext.GetAllAsync<Module>(), "ModuleID", "ModuleName");
            Data = await _dbContext.GetAsync<User>(id);
            PersonalInfo = await _dbContext.GetAsync<PersonalInfo_Client>(Convert.ToInt32(Data.PersonalInfoID));
            UserModules = (await _dbContext.GetEnumerableAsync<UserModule>($@"SELECT  U.UserModuleID,U.CanAccess,M.ModuleID
                FROM Module M
                LEFT JOIN UserModule U on M.ModuleID=U.ModuleID and U.IsDeleted=0 and U.UserID=@UserID", new { UserID =id})).ToList();
            Documents = new List<DocumentPostViewModel>();
            Company = await _dbContext.GetAsyncByFieldName<Louvre.Shared.Core.Company>("PersonalInfoID", Convert.ToInt32(Data.PersonalInfoID).ToString());

            if (Data.UserTypeID == (int)UserTypes.Company)
            {
                if (Company != null)
                    Documents = (await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, Convert.ToInt32(Company.CompanyID))).Where(s => !string.IsNullOrEmpty(s.FileName)).ToList();

            }
            else if (Data.UserTypeID == (int)UserTypes.Individual)
            {
                var employee = await _dbContext.GetAsyncByFieldName<Louvre.Shared.Core.Employee>("PersonalInfoID", Convert.ToInt32(Data.PersonalInfoID).ToString());
                if (employee != null)
                    Documents = (await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, Convert.ToInt32(employee.EmployeeID))).Where(s => !string.IsNullOrEmpty(s.FileName)).ToList();
            }
            else
            {
                Data = null;
                PersonalInfo = null;
            }
        }


        public async Task<IActionResult> OnPostSaveAsync()
        {
            var userData = await _dbContext.GetAsync<User>(Data.UserID.Value);
            userData.PersonalInfoID = PersonalInfo.PersonalInfoID;
            userData.EmailAddress = PersonalInfo.Email1;
            userData.MobileNumber = PersonalInfo.Phone1;
            userData.UserName = PersonalInfo.Email1;
            userData.LoginStatus = Data.LoginStatus;

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

                    await _dbContext.SaveAsync(userData, tran);

                    await _dbContext.SaveSubListAsync(UserModules, "UserID", Data.UserID.Value);

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