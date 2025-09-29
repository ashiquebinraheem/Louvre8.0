using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Progbiz
{
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    public class AreaModel : PageModel
    {
        private readonly IDbContext _dbContext;

        public AreaModel(IDbContext entity)
        {
            _dbContext = entity;
        }

        [BindProperty]
        public List<Area> Data { get; set; }

        public async Task OnGetAsync()
        {
            Data = (await _dbContext.GetAllAsync<Area>()).ToList();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();
            await _dbContext.SaveListAsync(Data);
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }
    }
}