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
    [Authorize(Roles = "Super-Admin,Administrator")]
    public class UsersModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public UsersModel(IDbContext entity)
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
                new SearchByViewModel("UserName", "Login ID"),
                new SearchByViewModel("EmailAddress", "Email Address"),
                new SearchByViewModel("MobileNumber", "Mobile Number")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "UserName",
                "EmailAddress",
                "MobileNumber",
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            var userType = await _dbContext.GetAsync<UserType>(CurrentUserTypeID);

            SearchData.Query = $@"Select U.* 
            From Users U
            JOIN UserTypes T on T.UserTypeID=U.UserTypeID";

            SearchData.WhereCondition = $@"ISNULL(IsDeleted,0)=0 and PriorityOrder>{userType.PriorityOrder} and ISNULL(T.ShowInList,1)=1";

            var result = await _dbContext.GetPagedList<User>(SearchData);
            return new JsonResult(result);
        }

    }
}