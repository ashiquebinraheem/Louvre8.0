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
    public class SlotModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly IErrorLogRepository _errorLogRepo;

        public SlotModel(IDbContext dbContext, IDbConnection cn, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _errorLogRepo = errorLogRepo;
        }

        public SlotMaster Data { get; set; }
        public List<SlotPostViewModel> Items { get; set; }
        public async Task OnGetAsync(int id)
        {
            Data = await _dbContext.GetAsync<SlotMaster>(Convert.ToInt32(id));
            Items = (await _dbContext.GetEnumerableAsync<SlotPostViewModel>($@"Select V.SlotID,TimeFrom,TimeTO,V.RequestCount,RequestedCount 
                from viSlot V
                LEFT JOIN Slot S on S.SlotID=V.SlotID
                where V.SlotMasterID=@SlotMasterID", new { SlotMasterID =id})).ToList();
            Items = Items ?? new List<SlotPostViewModel>();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var isExist = await _dbContext.GetAsyncByFieldName<SlotMaster>("Date", Data.Date.ToString());
            if (isExist != null && isExist.SlotMasterID != Data.SlotMasterID)
            {
                var response = new BaseResponse(-101);
                return new JsonResult(response);
            }

            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    //var id = await _dbContext.SaveAsync(Data, tran);

                    List<Slot> slots = new List<Slot>();
                    foreach (var item in Items)
                    {
                        Slot slot = new Slot()
                        {
                            SlotID = item.SlotID,
                            TimeFrom = item.TimeFrom,
                            TimeTo = item.TimeTo
                        };
                        if (item.RequestCount < item.RequestedCount)
                            slot.RequestCount = item.RequestedCount;
                        else
                            slot.RequestCount = item.RequestCount;
                        slots.Add(slot);
                    }

                    await _dbContext.SaveSubListAsync(slots, "SlotMasterID", Data.SlotMasterID.Value, tran);
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


        //Before deleting, check any of the slot is used
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            BaseResponse result = new BaseResponse();

            var usedSlotCount = await _dbContext.GetAsync<int>($"Select COUNT(*) from viSlot where SlotMasterID=@SlotMasterID and RequestedCount>0", new { SlotMasterID =Data.SlotMasterID});
            if (usedSlotCount > 0)
            {
                result.CreatErrorResponse(-108);
            }
            else
            {
                await _dbContext.DeleteAsync<SlotMaster>(Convert.ToInt32(Data.SlotMasterID));
                await _dbContext.DeleteSubItemsAsync<Slot>("SlotMasterID", Convert.ToInt32(Data.SlotMasterID));
                result.CreatSuccessResponse();
            }
            return new JsonResult(result);
        }

    }
}
