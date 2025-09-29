using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages.Approver
{
    [Authorize(Roles = "Approver,Disposal")]
    public class NewDailyPassRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public NewDailyPassRequestsModel(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public PagedListSearchPostModel SearchData { get; set; }


        public void OnGetAsync()
        {
            SearchData = new PagedListSearchPostModel();

            ViewData["GridColumns"] = new List<SearchByViewModel>()
            {
                new SearchByViewModel("RequestNo", "Request No"),
                new SearchByViewModel("FromDate", "From Date", "180px"),
                new SearchByViewModel("ToDate", "To Date", "180px"),
                new SearchByViewModel("EmployeeName", "Requester", "150px"),
                new SearchByViewModel("CompanyName", "Company","150px"),
                new SearchByViewModel("RequestTypeName", "Request Type", "150px"),
                new SearchByViewModel("BranchName", "Branch", "150px"),
                new SearchByViewModel("SubBranchName", "Sub Branch", "150px"),
                new SearchByViewModel("ModeName", "Mode", "150px"),
                new SearchByViewModel("RequestedLocationName", "Requested Location", "150px"),
                new SearchByViewModel("LocationName", "Approved Location", "150px"),
                 new SearchByViewModel("QRCode", "QR Code No","150px")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "RequestNo",
                "Date",
                "EmployeeName",
                "CompanyName",
                "RequestTypeName",
                "BranchName",
                "SubBranchName",
                "RequestedSlot",
                "ModeName",
                "RequestedLocationName",
                "Slot",
                "LocationName",
                "QRCode"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"SELECT DailyPassRequestID, RequestNo, 
			Convert(varchar, FromDate,103) FromDate, Convert(varchar, ToDate,103) ToDate,R.EmployeeName, BranchName, 
            SubBranchName,  ModeName, 
            RequestedLocationName,  LocationName, StatusID, T.RequestTypeName, CompanyName,QRCode
            FROM  viDailyPassRequest R
            JOIN Employee E on E.EmployeeID= R.EmployeeID
			JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID and S.Stage=ISNULL(R.ApprovalStage,0)+1
            JOIN UserTypes U on U.UserTypeID=S.UserTypeID
			LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
			LEFT JOIN RequestTypeApprovalStage S1 on S1.RequestTypeID=R.RequestTypeID and S1.Stage=ISNULL(R.ApprovalStage,0)+2";

            SearchData.WhereCondition = $@"IsRejected=0 and ISNULL(NeedHigherLevelApproval,1)=1 and CASE WHEN U.UserNature=2 and R.IsDisposalRequired=0 then S1.UserTypeID else S.UserTypeID end={CurrentUserTypeID} and FromDate>=CAST(GETDATE() AS DATE)";

            var result = await _dbContext.GetPagedList<DailyPassRequestListViewModel>(SearchData);
            return new JsonResult(result);
        }
    }
}
