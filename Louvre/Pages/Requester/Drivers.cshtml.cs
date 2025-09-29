using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Company,Individual")]
    public class DriversModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public DriversModel(IDbContext entity)
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
                new SearchByViewModel("EmployeeName", "Requester/Driver Name"),
                new SearchByViewModel("Email", "Email"),
                new SearchByViewModel("ContactNumber", "Phone Number")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "EmployeeName",
                "Email",
                "ContactNumber"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion


            SearchData.Query = $@"Select EmployeeID, EmployeeName, Email, ContactNumber 
            From Employee";

            SearchData.WhereCondition = $@"ISNULL(IsDeleted,0)=0 and AddedBy={CurrentUserID}";

            var result = await _dbContext.GetPagedList<Employee>(SearchData);
            return new JsonResult(result);
        }

    }
}