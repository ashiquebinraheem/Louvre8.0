using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Helpers;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Louvre.Pages.Product
{
    public class ImportItemsModel : PageModel
    {

        private readonly IDbContext _dbContext;
        private readonly IReflexionRepository _reflexion;

        public ImportItemsModel(IDbContext dbContext, IReflexionRepository reflexion)
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
                new SearchByViewModel("Type", "Item Type"),
                new SearchByViewModel("Code", "Item Code", "100px"),
                new SearchByViewModel("Code", "Item Name", "150px")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "Type",
                "Code",
                "Code"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select Code,Name,Type 
                from ItemMaster";
            
            var result = await _dbContext.GetPagedList<ItemListViewModel>(SearchData);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();
            await _reflexion.ImportItems();
            result.CreatSuccessResponse();
            return new JsonResult(result);
        }
    }
}
