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
    [Authorize(Roles = "Meterial")]
    public class AcceptedEntryRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public AcceptedEntryRequestsModel(IDbContext entity)
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
                new SearchByViewModel("R.EmployeeName", "Requester", "150px"),
                new SearchByViewModel("CompanyName", "Company","150px"),
                new SearchByViewModel("RequestTypeName", "Request Type", "150px"),
                new SearchByViewModel("BranchName", "Branch", "150px"),
                new SearchByViewModel("SubBranchName", "Sub Branch", "150px"),
                new SearchByViewModel("RequestedSlot", "Requested Slot", "180px"),
                new SearchByViewModel("ModeName", "Mode", "150px"),
                new SearchByViewModel("RequestedLocationName", "Requested Location", "150px"),
                //new SearchByViewModel("Status", "Status","100px",false),
                new SearchByViewModel("Slot", "Approved Slot", "180px"),
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
                "Status",
                "Slot",
                "LocationName",
                "QRCode"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            // Static query (safe)
            SearchData.Query = @"SELECT RequestID, RequestNo, CONVERT(varchar, RequestedOn,103) AS Date, 
                       R.EmployeeName, BranchName, SubBranchName, RequestedSlot, ModeName, 
                       RequestedLocationName, Slot, LocationName, StatusID, 
                       T.RequestTypeName, CompanyName, QRCode
                FROM viRequest R
                JOIN Employee E ON E.EmployeeID = R.EmployeeID
                LEFT JOIN RequestType T ON R.RequestTypeID = T.RequestTypeID";

            // Safe where condition
            SearchData.WhereCondition = $@"StatusID = {(int)RequestStatus.Accepted}
                AND IsIn = 1
                AND RequestedByID = {Convert.ToInt32(CurrentUserID)}
                AND ISNULL(IsProjectAsset,0) = 0";

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