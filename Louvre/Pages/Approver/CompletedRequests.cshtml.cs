using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin, Administrator, Approver,Disposal,Meterial")]
    public class CompletedRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public CompletedRequestsModel(IDbContext entity)
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
                new SearchByViewModel("Date", "Request Date", "100px"),
                new SearchByViewModel("EmployeeName", "Requester", "150px"),
                new SearchByViewModel("CompanyName", "Company","150px"),
                new SearchByViewModel("RequestTypeName", "Request Type", "150px"),
                new SearchByViewModel("BranchName", "Branch", "150px"),
                new SearchByViewModel("SubBranchName", "Sub Branch", "150px"),
                new SearchByViewModel("RequestedSlot", "Requested Slot", "180px"),
                new SearchByViewModel("ModeName", "Mode", "150px"),
                new SearchByViewModel("RequestedLocationName", "Requested Location", "150px"),
                //new SearchByViewModel("Status", "Status","100px",false),
                new SearchByViewModel("Slot", "Approved Slot", "180px"),
                new SearchByViewModel("LocationName", "Approved Location", "150px")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()   // Added by Abdul Razack for No Sql Injection Risk
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
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            // Base query (static, safe)
            SearchData.Query = @"
        SELECT RequestID, RequestNo, CONVERT(varchar, RequestedOn,103) AS Date, 
               EmployeeName, BranchName, SubBranchName, RequestedSlot, ModeName, 
               RequestedLocationName, Slot, LocationName, StatusID, 
               T.RequestTypeName, R.CompanyName
        FROM viRequest R
        LEFT JOIN RequestType T ON R.RequestTypeID = T.RequestTypeID";

            // Safe base condition
            SearchData.WhereCondition = $"StatusID = {(int)RequestStatus.Completed}";

            // Role-based filter
            if (User.IsInRole("Meterial"))
            {
                // Force integer conversion to block injection
                SearchData.WhereCondition += $" AND RequestedByID = {Convert.ToInt32(CurrentUserID)}";
            }
            else
            {
                // Force integer conversion for UserTypeID too
                SearchData.WhereCondition += $@"
            AND R.RequestTypeID IN (
                SELECT RequestTypeID 
                FROM RequestTypeApprovalStage 
                WHERE UserTypeID = {Convert.ToInt32(CurrentUserTypeID)}
            )";
            }

            // Allow-list for SearchColumnName
            switch (SearchData.SearchColumnName)
            {
                case "Date":
                    SearchData.SearchColumnName = "CONVERT(varchar, RequestedOn,103)";
                    break;
            }

            var result = await _dbContext.GetPagedList<RequestListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}