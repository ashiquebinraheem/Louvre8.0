using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using QRCoder;
using System;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    public class ApproveVisitRequestMailModel : PageModel
    {
        protected readonly IDbContext _dbContext;
        private readonly ICommonRepository _commonRepository;

        public ApproveVisitRequestMailModel(IDbContext entity, ICommonRepository commonRepository)
        {
            _dbContext = entity;
            _commonRepository = commonRepository;
        }

        public async Task OnGetAsync(int id, string qrcode)
        {
            TempData["Success"] = "";
            var request = await _dbContext.GetAsync<int?>($@"select VisitRequestID from viVisitRequest where VisitRequestID=@VisitRequestID and QRCode=@QRCode and ISNULL(IsApproved,0)=0 and ISNULL(IsRejected,0)=0", new { VisitRequestID =id, QRCode =qrcode});
            if (request == null)
            {
                TempData["Message"] = "Invaid link";
            }
            else
            {
                await _dbContext.ExecuteAsync($"Update VisitRequest set IsApproved=1,ApprovedOn=@ApprovedOn where VisitRequestID=@VisitRequestID", new { VisitRequestID =id, ApprovedOn = DateTime.UtcNow});

                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                await _commonRepository.SendVisitorRequestApprovalMail(id, url);

                var result = new BaseResponse(107);
                TempData["Message"] = result.ResponseMessage;
                if (result.ResponseCode >= 0)
                {
                    TempData["Success"] = "Success";
                }
            }
        }
    }
}