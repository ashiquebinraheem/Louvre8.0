using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin,Administrator")]
    [BindProperties]
    public class EmailSettingsModel : BasePageModel
    {
        private readonly IDbContext _dbContext;


        public EmailSettingsModel(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public MailSettings Data { get; set; }

        public async Task OnGetAsync()
        {
            Data = await _dbContext.GetAsync<MailSettings>(Convert.ToInt32(1));
            Data.Password = "";
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();

            var currentEntry = await _dbContext.GetAsync<MailSettings>(1);
            if (string.IsNullOrEmpty(Data.Password) || Data.Password== "*******")
            {
                Data.Password = currentEntry.Password;
            }
            await _dbContext.SaveAsync(Data);
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }
    }
}