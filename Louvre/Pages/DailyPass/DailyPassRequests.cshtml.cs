using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Meterial")]
    public class DailyPassRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public DailyPassRequestsModel(IDbContext entity)
        {
            _dbContext = entity;
        }

        [BindProperty]
        public PagedListSearchPostModel SearchData { get; set; }


        public void OnGetAsync()
        {
            SearchData = new PagedListSearchPostModel();

            ViewData["GridColumns"] = new List<SearchByViewModel>()
            {
                new SearchByViewModel("RequestNo", "Request No"),
                new SearchByViewModel("FromDate", "From Date","180px",false),
                new SearchByViewModel("ToDate", "To Date","180px",false),
                new SearchByViewModel("EmployeeName", "Employee","150px"),
                new SearchByViewModel("CompanyName", "Company","150px"),
                new SearchByViewModel("RequestTypeName", "Request Type", "150px"),
                new SearchByViewModel("BranchName", "Branch","150px"),
                new SearchByViewModel("SubBranchName", "Sub Branch","150px"),
                new SearchByViewModel("ModeName", "Mode","150px"),
                new SearchByViewModel("RequestedLocationName", "Requested Location","150px"),
                new SearchByViewModel("Status", "Status","100px",false),
                new SearchByViewModel("LocationName", "Approved Location","150px"),
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
            LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID";

            SearchData.WhereCondition = $@"RequestedByID={CurrentUserID} and IsIn=1";

            var result = await _dbContext.GetPagedList<DailyPassRequestListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}