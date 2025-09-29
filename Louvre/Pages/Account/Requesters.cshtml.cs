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
    public class RequestersModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public RequestersModel(IDbContext entity)
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
                new SearchByViewModel("Name", "Requester"),
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

            SearchData.Query = $@"SELECT  UserID,coalesce(P.FirstName,UserName) as Name, EmailAddress, MobileNumber
            FROM Users U
            LEFT JOIN PersonalInfos P on P.PersonalInfoID=U.PersonalInfoID";

            SearchData.WhereCondition = $@"ISNULL(U.IsDeleted,0)=0 and EmailConfirmed=1 and UserTypeID in({(int)UserTypes.Company},{(int)UserTypes.Individual}) and ISNULL(IsApproved,0)=1";

            var result = await _dbContext.GetPagedList<UserApproveListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}