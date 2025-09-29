using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    [BindProperties]
    public class SlotPatternModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly IErrorLogRepository _errorLogRepo;

        public SlotPatternModel(IDbContext dbContext, IDbConnection cn, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _errorLogRepo = errorLogRepo;
        }

        public SlotPattern Data { get; set; }
        public List<SlotPatternItem> Items { get; set; }
        public async Task OnGetAsync(int? id)
        {
            ViewData["SlotGroups"] = new SelectList((await _dbContext.GetAllAsync<SlotGroup>()).ToList(), "SlotGroupID", "SlotGroupName");
            if (id != null)
            {
                Data = await _dbContext.GetAsync<SlotPattern>(Convert.ToInt32(id));
                Items = (await _dbContext.GetAllAsyncByFieldName<SlotPatternItem>("SlotPatternID", id.ToString())).ToList();
            }

            if (Items == null)
                Items = new List<SlotPatternItem>();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var isExist = await _dbContext.GetAsyncByFieldName<SlotPattern>("SlotPatternName", Data.SlotPatternName);
            if (isExist != null && isExist.SlotPatternID != Data.SlotPatternID)
            {
                var response = new BaseResponse(-103);
                return new JsonResult(response);
            }

            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        Items[i].DayNo = i + 1;
                    }

                    var id = await _dbContext.SaveAsync(Data, tran);
                    await _dbContext.SaveSubListAsync(Items, "SlotPatternID", id, tran);
                    tran.Commit();
                    result.CreatSuccessResponse(1);
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                }

                return new JsonResult(result);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            BaseResponse result = new BaseResponse();
            await _dbContext.DeleteAsync<SlotPattern>(Convert.ToInt32(Data.SlotPatternID));
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }

    }
}
