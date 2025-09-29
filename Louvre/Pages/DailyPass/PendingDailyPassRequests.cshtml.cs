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
    public class PendingDailyPassRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public PendingDailyPassRequestsModel(IDbContext entity)
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

        //     public async Task<IActionResult> OnPostSearchAsync()
        //     {
        //         try { 
        //         SearchData.Query = $@"SELECT DailyPassRequestID, RequestNo, Convert(varchar, RequestedOn,103) Date, 
        //Convert(varchar, FromDate,103) FromDate, Convert(varchar, ToDate,103) ToDate,R.EmployeeName, BranchName, 
        //         SubBranchName,  ModeName, 
        //         RequestedLocationName,  LocationName, StatusID, T.RequestTypeName, CompanyName,QRCode
        //         FROM  viDailyPassRequest R
        //         JOIN Employee E on E.EmployeeID= R.EmployeeID
        //         LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID";

        //         SearchData.WhereCondition = $@"StatusID in ({(int)RequestStatus.Pending},{(int)RequestStatus.Processing},{(int)RequestStatus.Forwarded}) and IsIn=1";

        //         if (User.IsInRole("Meterial"))
        //             SearchData.WhereCondition += $@" and RequestedByID={CurrentUserID}";
        //         else
        //             SearchData.WhereCondition += $" and R.RequestTypeID in(Select RequestTypeID from RequestTypeApprovalStage Where UserTypeID={CurrentUserTypeID})";


        //         switch (SearchData.SearchColumnName)
        //         {
        //             case "Date":
        //                 SearchData.SearchColumnName = "Convert(varchar, RequestedOn,103)";
        //                 break;
        //         }
        //         var result = await _dbContext.GetPagedList<DailyPassRequestListViewModel>(SearchData);
        //         return new JsonResult(result);

        //         }
        //         catch (Exception ex)
        //         { 
        //             throw ex; 
        //         }
        //     }

        public async Task<IActionResult> OnPostSearchAsync()    // Added by Abdul Razack for No Sql Injection Risk
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

            try
            {
                // Static base query
                SearchData.Query = @"
            SELECT DailyPassRequestID, RequestNo, CONVERT(varchar, RequestedOn,103) AS Date,
                   CONVERT(varchar, FromDate,103) AS FromDate,
                   CONVERT(varchar, ToDate,103) AS ToDate,
                   R.EmployeeName, BranchName, SubBranchName, ModeName,
                   RequestedLocationName, LocationName, StatusID,
                   T.RequestTypeName, CompanyName, QRCode
            FROM viDailyPassRequest R
            JOIN Employee E ON E.EmployeeID = R.EmployeeID
            LEFT JOIN RequestType T ON R.RequestTypeID = T.RequestTypeID";

                // Safe base WhereCondition
                SearchData.WhereCondition = $@"
            StatusID IN ({(int)RequestStatus.Pending}, {(int)RequestStatus.Processing}, {(int)RequestStatus.Forwarded})
            AND IsIn = 1";

                // Role-based filtering
                if (User.IsInRole("Meterial"))
                {
                    SearchData.WhereCondition += $" AND RequestedByID = {Convert.ToInt32(CurrentUserID)}";
                }
                else
                {
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
                        break;;
                }

                var result = await _dbContext.GetPagedList<DailyPassRequestListViewModel>(SearchData);
                return new JsonResult(result);
            }
            catch (Exception)
            {
                // Better: preserve stack trace with "throw;"
                throw;
            }
        }


    }
}