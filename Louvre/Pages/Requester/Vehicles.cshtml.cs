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
    [Authorize(Roles = "Company,Individual")]
    public class VehiclesModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public VehiclesModel(IDbContext entity)
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
                new SearchByViewModel("RegisterNo", "Register No"),
                new SearchByViewModel("VehicleTypeName", "Vehicle Type","",false),
                new SearchByViewModel("VehicleSize", "Vehicle Size"),
                new SearchByViewModel("PlateNo", "Plate No")
            };
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            #region Validation

            List<string> validFields = new()
            {
                "RegisterNo",
                "VehicleTypeName",
                "VehicleSize",
                "PlateNo"
            };

            SearchValidationHelper.ValidateSearchData(SearchData.SearchColumnName, SearchData.OrderByFieldName, validFields);

            #endregion

            SearchData.Query = $@"Select VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo 
            From Vehicle V
            LEFT JOIN VehicleType T on V.VehicleTypeID=T.VehicleTypeID";

            SearchData.WhereCondition = $@"ISNULL(V.IsDeleted,0)=0 and V.AddedBy={CurrentUserID}";

            var result = await _dbContext.GetPagedList<VehicleListViewModel>(SearchData);
            return new JsonResult(result);
        }

    }
}