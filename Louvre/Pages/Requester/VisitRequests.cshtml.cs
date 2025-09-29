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
    [Authorize(Roles = "Visitor")]
    public class VisitRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public VisitRequestsModel(IDbContext entity)
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
                new SearchByViewModel("MeetingDate", "Meeting Date", "150px"),
                new SearchByViewModel("Status", "Status", "150px"),
                new SearchByViewModel("EmployeeName", "Host", "150px"),
                new SearchByViewModel("DurationName", "Duration", "100px"),
                new SearchByViewModel("PurposeName", "Purpose", "200px"),
                new SearchByViewModel("DepartmentName", "Department", "180px"),
                new SearchByViewModel("AreaName", "Area", "150px"),
                new SearchByViewModel("Remark", "Remark", "300px"),
            };
        }


        public async Task<IActionResult> OnPostSearchAsync()    // Added by Abdul Razack for No Sql Injection 
        {
            #region Validation

            List<string> validFields = new()
            {
                "MeetingDate",
                "Status",
                "EmployeeName",
                "DurationName",
                "PurposeName",
                "DepartmentName",
                "AreaName",
                "Remark"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            // Base query
            SearchData.Query = @"
            SELECT VisitRequestID, EmployeeName, DepartmentName, AreaName, PurposeName, 
                   CONVERT(varchar, MeetingDate, 100) AS MeetingDate, DurationName, 
                   ISNULL(Remark,'') AS Remark, StatusID
            FROM viVisitRequest";

            // Sanitize: always force CurrentUserID to int
            SearchData.WhereCondition = $"RequestedByID = {Convert.ToInt32(CurrentUserID)}";

            // Allow-list for OrderByFieldName (to avoid injection risk)
            var validOrderBy = new List<string> { "StatusID", "MeetingDate", "EmployeeName" };

            if (!validOrderBy.Contains(SearchData.OrderByFieldName))
            {
                // fallback to safe default column
                SearchData.OrderByFieldName = "MeetingDate";
            }

            // Execute paginated query
            var result = await _dbContext.GetPagedList<VisitRequestListViewModel>(SearchData);

            return new JsonResult(result);
        }


    }
}