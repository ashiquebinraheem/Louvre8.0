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
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    public class EmployeesModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public EmployeesModel(IDbContext entity)
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
                new SearchByViewModel("Name", "Name"),
                new SearchByViewModel("EmailAddress", "Email Address"),
                new SearchByViewModel("MobileNumber", "Mobile Number")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "Name",
                "EmailAddress",
                "MobileNumber",
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"SELECT U.UserID, P.Name, EmailAddress, MobileNumber
            FROM  Users U
            JOIN viPersonalInfos P on P.UserID=U.UserID";

            SearchData.WhereCondition = $@"U.UserTypeID={(int)UserTypes.Employee}";

            var result = await _dbContext.GetPagedList<EmployeeListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}