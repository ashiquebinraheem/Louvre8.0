using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Progbiz
{
    [Authorize]
    public class SubBranchModel : PageModel
    {
        private readonly IDbContext _dbContext;

        public SubBranchModel(IDbContext entity)
        {
            _dbContext = entity;
        }

        [BindProperty]
        public List<Branch> Data { get; set; }

        public async Task OnGetAsync()
        {
            Data = (await _dbContext.GetAllAsync<Branch>()).ToList().Where(s => s.ParentBranchID != null).ToList();
            ViewData["Branches"] = new SelectList((await _dbContext.GetAllAsync<Branch>()).ToList().Where(s => s.ParentBranchID == null).ToList(), "BranchID", "BranchName");
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();
            await _dbContext.SaveListAsync(Data, "ParentBranchID is not null");
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }
    }
}