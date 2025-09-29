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
    public class SlotGroupsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public SlotGroupsModel(IDbContext entity)
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
                new SearchByViewModel("SlotGroupName", "Slot Group Name")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "SlotGroupName",
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select * 
            From SlotGroup";

            SearchData.WhereCondition = $@"ISNULL(IsDeleted,0)=0";

            var result = await _dbContext.GetPagedList<SlotGroup>(SearchData);
            return new JsonResult(result);
        }

    }
}