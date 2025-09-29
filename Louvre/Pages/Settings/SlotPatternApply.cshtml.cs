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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Super-Admin,Administrator,Approver,Disposal")]
    [BindProperties]
    public class SlotPatternApplyModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly IErrorLogRepository _errorLogRepo;

        public SlotPatternApplyModel(IDbContext dbContext, IDbConnection cn, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _errorLogRepo = errorLogRepo;
        }

        [Required]
        public int SlotPatternID { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }
        public List<Slot> Slots { get; set; }
        [Required]
        public int BranchID { get; set; }

        public List<SlotMaster> SlotMasters { get; set; }

        public async Task OnGetAsync()
        {
            FromDate = DateTime.Now;
            ToDate = DateTime.Now;
            ViewData["SlotGroups"] = (await _dbContext.GetAllAsync<SlotGroup>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.SlotGroupID), Value = s.SlotGroupName }).ToList();
            ViewData["SlotPatterns"] = new SelectList((await _dbContext.GetAllAsync<SlotPattern>()).ToList(), "SlotPatternID", "SlotPatternName");
            ViewData["Branches"] = new SelectList((await _dbContext.GetAllAsync<Branch>()).ToList().Where(s => s.ParentBranchID == null).ToList(), "BranchID", "BranchName");
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    foreach (var item in SlotMasters)
                    {
                        if (item.SlotGroupID != null)
                        {
                            item.BranchID = BranchID;
                            var id = await _dbContext.SaveAsync(item, tran);
                            Slots = (await _dbContext.GetEnumerableAsync<Slot>($"Select TimeFrom, TimeTo, RequestCount From SlotGroupItem Where ISNULL(IsDeleted,0)=0 and SlotGroupID={item.SlotGroupID}", null, tran)).ToList();
                            await _dbContext.SaveSubListAsync(Slots, "SlotMasterID", id, tran);
                        }
                    }
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


        public async Task<IActionResult> OnPostApplyAsync()
        {
            BaseResponse result = new BaseResponse();

            string cnt = await _dbContext.ExecuteScalarAsync<string>($@"SELECT  STRING_AGG(Date,',')
            FROM SlotMaster
            Where (Date between @FromDate and @ToDate) and BranchID={BranchID} and ISNULL(IsDeleted,0)=0", new { FromDate, ToDate });

            if (!string.IsNullOrEmpty(cnt))
            {
                result.CreatErrorResponse(-100, cnt);
                return new JsonResult(result);
            }

            SlotMasters = new List<SlotMaster>();
            var slotPatterItems = (await _dbContext.GetAllAsyncByFieldName<SlotPatternItem>("SlotPatternID", SlotPatternID.ToString())).ToList();

            int j = 0;
            for (var i = FromDate; i <= ToDate; i = i.AddDays(1), j++)
            {
                if (j == slotPatterItems.Count())
                    j = 0;

                SlotMasters.Add(new SlotMaster() { Date = i, SlotGroupID = slotPatterItems[j].SlotGroupID });
            }

            return new JsonResult(SlotMasters);
        }

    }
}
