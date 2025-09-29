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
    public class ExitRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public ExitRequestsModel(IDbContext entity)
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
                new SearchByViewModel("Date", "Date","",false),
                new SearchByViewModel("EmployeeName", "Employee","150px"),
                new SearchByViewModel("RequestTypeName", "Request Type", "150px"),
                new SearchByViewModel("CompanyName", "Company","150px"),
                new SearchByViewModel("BranchName", "Branch","150px"),
                new SearchByViewModel("SubBranchName", "Sub Branch","150px"),
                new SearchByViewModel("RequestedSlot", "Requested Slot","180px",false),
                new SearchByViewModel("ModeName", "Mode","150px"),
                new SearchByViewModel("RequestedLocationName", "Requested Location","150px"),
                new SearchByViewModel("Status", "Status","100px",false),
                new SearchByViewModel("Slot", "Approved Slot","180px",false),
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

            SearchData.Query = $@"SELECT RequestID, Convert(varchar, RequestedOn,103) Date, R.EmployeeName, BranchName, 
            SubBranchName, RequestedSlot, ModeName, 
            RequestedLocationName, Slot, LocationName, StatusID, T.RequestTypeName, CompanyName,QRCode
            FROM  viRequest R
            JOIN Employee E on E.EmployeeID= R.EmployeeID
            LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID";

            SearchData.WhereCondition = $@"RequestedByID={CurrentUserID} and IsIn=0";

            var result = await _dbContext.GetPagedList<RequestListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}