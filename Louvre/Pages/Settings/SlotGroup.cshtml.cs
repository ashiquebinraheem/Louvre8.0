using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class SlotGroupModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly IErrorLogRepository _errorLogRepo;

        public SlotGroupModel(IDbContext dbContext, IDbConnection cn, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _errorLogRepo = errorLogRepo;
        }

        public SlotGroup Data { get; set; }
        public List<SlotGroupItem> Items { get; set; }
        public async Task OnGetAsync(int? id)
        {
             if (id != null)
            {
                Data = await _dbContext.GetAsync<SlotGroup>(Convert.ToInt32(id));
                Items = (await _dbContext.GetAllAsyncByFieldName<SlotGroupItem>("SlotGroupID", id.ToString())).ToList();
            }

          //  if (Items == null)
            //   Items = new List<SlotGroupItem>();
            Items = Items ?? new List<SlotGroupItem>();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var isExist = await _dbContext.GetAsyncByFieldName<SlotGroup>("SlotGroupName", Data.SlotGroupName);
            if (isExist != null && isExist.SlotGroupID != Data.SlotGroupID)
            {
                var response = new BaseResponse(-102);
                return new JsonResult(response);
            }

            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    var id = await _dbContext.SaveAsync(Data, tran);
                    await _dbContext.SaveSubListAsync(Items, "SlotGroupID", id, tran);
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
            await _dbContext.DeleteAsync<SlotGroup>(Convert.ToInt32(Data.SlotGroupID));
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }

    }
}
