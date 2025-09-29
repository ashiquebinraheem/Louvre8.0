using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages.Product
{
    public class POOwnersModel : PageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IReflexionRepository _reflexion;

        public POOwnersModel(IDbContext dbContext, IReflexionRepository reflexion)
        {
            _dbContext = dbContext;
            _reflexion = reflexion;
        }

        [BindProperty]
        public PagedListSearchPostModel SearchData { get; set; }

        public void OnGetAsync()
        {
            SearchData = new PagedListSearchPostModel();

            ViewData["GridColumns"] = new List<SearchByViewModel>()
            {
                new SearchByViewModel("StaffName", "Staff Name"),
                new SearchByViewModel("Designation", "Designation"),
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "StaffName",
                "Designation"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select StaffName,Designation 
                from POOwner";

            var result = await _dbContext.GetPagedList<POOwner>(SearchData);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();
            await _reflexion.ImportPOOwners();
            result.CreatSuccessResponse();
            return new JsonResult(result);
        }
    }
}
