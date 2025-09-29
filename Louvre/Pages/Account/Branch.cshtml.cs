using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Client,Branch")]
    [BindProperties]
    public class BranchModel : BasePageModel
    {
        private readonly IDbContext _dbContext;

        public BranchModel(IDbContext entity)
        {
            _dbContext = entity;
        }

        public Branch Branch { get; set; }
        public async Task OnGetAsync(int? id)
        {
            if (id != null)
            {
                Branch = await _dbContext.GetAsync<Branch>(Convert.ToInt32(id));
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var isExist = await _dbContext.GetAsyncByFieldName<Branch>("BranchName", Branch.BranchName);
            if (isExist != null && isExist.BranchID != Branch.BranchID)
            {
                var response = new BaseResponse(-7);
                return new JsonResult(response);
            }
            BaseResponse result = new BaseResponse();
            await _dbContext.SaveAsync(Branch);
            result.CreatSuccessResponse(1);
            return new JsonResult(result);

        }
    }
}