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
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    public class SlotPatternsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public SlotPatternsModel(IDbContext entity)
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
                new SearchByViewModel("SlotPatternName", "Slot Pattern Name")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "SlotPatternName",
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select * 
            From SlotPattern ";

            SearchData.WhereCondition = $@"ISNULL(IsDeleted,0)=0";

            var result = await _dbContext.GetPagedList<SlotPattern>(SearchData);
            return new JsonResult(result);
        }

    }
}