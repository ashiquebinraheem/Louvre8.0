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
    public class SlotsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public SlotsModel(IDbContext entity)
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
                new SearchByViewModel("Date", "Date", "",false),
                new SearchByViewModel("SlotGroupName", "Slot Group Name"),
                new SearchByViewModel("BranchName", "Branch Name")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "Date",
                "SlotGroupName",
                "BranchName"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"SELECT SlotMasterID,CONVERT(varchar, Date,103) as Date, G.SlotGroupName, B.BranchName
            FROM  SlotMaster S
            LEFT JOIN SlotGroup G on G.SlotGroupID=S.SlotGroupID
            LEFT JOIN Branch B on B.BranchID=S.BranchID";

            SearchData.WhereCondition = $@"ISNULL(S.IsDeleted,0)=0 and Date>=CAST(GETDATE() AS date)";

            var result = await _dbContext.GetPagedList<SlotListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}