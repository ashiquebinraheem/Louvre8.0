using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin, Administrator, Approver,Disposal")]
    [BindProperties]
    public class RequestListModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public RequestListModel(IDbContext entity)
        {
            _dbContext = entity;
        }
        public PagedListSearchPostModel SearchData { get; set; }


        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int ModeID { get; set; }
        public int StatusID { get; set; }
        public int BranchID { get; set; }
        public int SubBranchID { get; set; }
        public int LocationID { get; set; }

        public async Task OnGetAsync()
        {
            SearchData = new PagedListSearchPostModel();

            ViewData["GridColumns"] = new List<SearchByViewModel>()
            {
                new SearchByViewModel("RequestNo", "Request No"),
                new SearchByViewModel("AllotedDate", "Date", "130px"),
                new SearchByViewModel("AllotedTime", "Time", "100px"),
                new SearchByViewModel("CompanyName", "Company", "150px"),
                new SearchByViewModel("RequestedBy", "Requestor", "150px",false),
                new SearchByViewModel("EmployeeName", "Employee", "150px",false),
                new SearchByViewModel("BranchName", "Branch", "150px"),
                new SearchByViewModel("SubBranchName", "Sub Branch", "150px"),
                new SearchByViewModel("ModeName", "Mode", "150px"),
                new SearchByViewModel("LocationName", "Location", "150px"),
                new SearchByViewModel("StorageLocation", "Storage Location", "180px")
            };

            ViewData["Branches"] = await _dbContext.GetAllAsync<Branch>();
            ViewData["RequestModes"] = await GetSelectList<RequestMode>(_dbContext, "ModeName");
            ViewData["Locations"] = await GetSelectList<Location>(_dbContext, "LocationName");
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {

            SetQuery();

            var result = await _dbContext.GetPagedList<ScheduleList>(SearchData);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostExportReportAsync()
        {
           
            SetQuery();

            var result = await _dbContext.GetSearchList<ScheduleList>(SearchData);
            return new JsonResult(result);
        }

        private void SetQuery()
        {
            #region Validation

            List<string> validFields = new()
            {
                "RequestNo",
                "AllotedDate",
                "AllotedTime",
                "CompanyName",
                "RequestedBy",
                "EmployeeName",
                "BranchName",
                "SubBranchName",
                "ModeName",
                "LocationName",
                "StorageLocation"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"SELECT  Convert(varchar,AllotedDate,103) AllotedDate, R.RequestNo, AllotedTime, R.EmployeeName, BranchName,SubBranchName, ModeName,RequestedBy, R.LocationName, L.LocationName as StorageLocation, C.CompanyName
            FROM viRequest R
            LEFT JOIN Location L on L.LocationID=R.StorageLocationID
            LEFT JOIN Employee E on E.EmployeeID=R.EmployeeID
            LEFT JOIN Company C on C.CompanyID=E.CompanyID";

            SearchData.WhereCondition = $@"(R.StatusID={StatusID} or  {StatusID}=0)
                and (RequestModeID={ModeID} or {ModeID}=0)
                and (BranchID={BranchID} or {BranchID}=0) 
                and (SubBranchID={SubBranchID} or {SubBranchID}=0)
                and (R.LocationID={LocationID} or {LocationID}=0)";

            if (FromDate != null)
                SearchData.WhereCondition += $" and AllotedDate>='{FromDate.Value.ToString(SQLDateFormate)}'";

            if (ToDate != null)
                SearchData.WhereCondition += $" and AllotedDate<='{ToDate.Value.ToString(SQLDateFormate)}'";
        }

    }
}