using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Progbiz
{
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    public class DocumentTypeModel : PageModel
    {
        private readonly IDbContext _dbContext;
        private readonly ICommonRepository _common;

        public DocumentTypeModel(IDbContext entity, ICommonRepository common)
        {
            _dbContext = entity;
            _common = common;
        }

        [BindProperty]
        public List<DocumentType> Data { get; set; }

        public async Task OnGetAsync()
        {
            Data = (await _dbContext.GetAllAsync<DocumentType>()).ToList();
            ViewData["Categories"] = new SelectList(_common.GetDocumentTypeCategories(), "ID", "Value");
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