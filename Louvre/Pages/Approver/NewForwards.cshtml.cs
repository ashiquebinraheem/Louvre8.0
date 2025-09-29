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
    [Authorize(Roles = "Security-Control-Room, Security-Duty-Manager")]
    public class NewForwardsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public NewForwardsModel(IDbContext entity)
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
                new SearchByViewModel("Remarks", "Remarks", "150px")
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
                "Remarks",
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"SELECT RequestID, RequestNo, Convert(varchar, RequestedOn,103) Date, EmployeeName, Remarks, CompanyName
            FROM  viRequest R";

            SearchData.WhereCondition = $@"StatusID=5";

            var result = await _dbContext.GetPagedList<RequestForwardListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}