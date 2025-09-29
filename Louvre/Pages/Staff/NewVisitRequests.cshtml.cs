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
    [Authorize(Roles = "Employee")]
    public class NewVisitRequestsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public NewVisitRequestsModel(IDbContext entity)
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
                new SearchByViewModel("Requester", "Requester", "150px"),
                new SearchByViewModel("DurationName", "Duration", "100px"),
                new SearchByViewModel("PurposeName", "Purpose", "200px"),
                new SearchByViewModel("DepartmentName", "Department", "180px"),
                new SearchByViewModel("AreaName", "Area", "150px"),
                new SearchByViewModel("Remark", "Remark", "300px"),
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "MeetingDate",
                "Requester",
                "DurationName",
                "PurposeName",
                "DepartmentName",
                "AreaName",
                "Remark"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select VisitRequestID, Requester, DepartmentName, AreaName, PurposeName, convert(varchar, MeetingDate, 100) MeetingDate, DurationName, ISNULL(Remark,'') as Remark
            From viVisitRequest";

            SearchData.WhereCondition = $@"StatusID={(int)RequestStatus.Pending} and HostUserID={CurrentUserID}";

            var result = await _dbContext.GetPagedList<VisitRequestListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}