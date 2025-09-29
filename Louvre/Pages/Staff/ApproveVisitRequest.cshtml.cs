using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Employee")]
    [BindProperties]
    public class ApproveVisitRequestModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public ApproveVisitRequestModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _errorLogRepo = errorLogRepo;
        }

        public VisitRequest Data { get; set; }
        public List<VisitRequestDrink> Drinks { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }

        public async Task OnGetAsync(int id)
        {
            Data = await _dbContext.GetAsync<VisitRequest>(id);

            if (Data == null || Data.HostUserID != CurrentUserID || Data.IsApproved == true || Data.IsRejected == true)
            {
                Data = null;
                RedirectToPage("/Employee/NewVisitRequests");
            }

            int requesterId = Convert.ToInt32(Data.AddedBy);

            var employees = await _commonRepository.GetEmployeesAsync(requesterId);
            ViewData["Employees"] = employees;
            ViewData["Countries"] = await GetSelectList<Country>(_dbContext, "CountryName");
            ViewData["Departments"] = await GetSelectList<Department>(_dbContext, "DepartmentName");
            ViewData["Areas"] = await GetSelectList<Area>(_dbContext, "AreaName");
            ViewData["Purposes"] = await GetSelectList<Purpose>(_dbContext, "PurposeName");
            ViewData["Durations"] = await GetSelectList<Duration>(_dbContext, "DurationName");
            ViewData["Vehicles"] = await GetSelectList<Vehicle>(_dbContext, "RegisterNo", $"AddedBy ={ requesterId }");
            Documents = await _commonRepository.GetAllVisitRequestDocumentsAsync(Convert.ToInt32(Data.VisitRequestID));
        }




        public async Task<IActionResult> OnPostApproveAsync()
        {
            BaseResponse result = new BaseResponse();

            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    Data.HostUserID = CurrentUserID;
                    Data.IsApproved = true;
                    Data.ApprovedBy = CurrentUserID;
                    Data.ApprovedOn = DateTime.UtcNow;
                    var requestId = await _dbContext.SaveAsync(Data, tran);
                    await _dbContext.SaveSubListAsync(Drinks, "VisitRequestID", requestId, tran);

                    var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    await _commonRepository.SendVisitorRequestApprovalMail(requestId, url, tran);

                    tran.Commit();
                    result.CreatSuccessResponse(102);
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                }

                return new JsonResult(result);
            }
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            BaseResponse result = new BaseResponse();
            try
            {
                Data.IsRejected = true;
                await _dbContext.SaveAsync(Data);
                result.CreatSuccessResponse(104);
            }
            catch (Exception err)
            {
                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
            }
            return new JsonResult(result);
        }

    }
}