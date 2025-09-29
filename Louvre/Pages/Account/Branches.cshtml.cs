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
    [Authorize(Roles = "Client,Branch")]
    public class BranchesModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public BranchesModel(IDbContext entity)
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
                new SearchByViewModel("BranchName", "BranchName")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {

            #region Validation

            List<string> validFields = new()
            {
                "BranchName",
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select * From Branches";
            SearchData.WhereCondition = "ISNULL(IsDeleted,0)=0 and ParentBranchID is null";
            var result = await _dbContext.GetPagedList<Branch>(SearchData);
            return new JsonResult(result);
        }

    }
}