using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace Louvre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IMediaRepository _media;

        public RequestController(IDbContext dbContext, IMediaRepository media)
        {
            _dbContext = dbContext;
            _media = media;
        }

        [HttpGet("get-vendor-details/{companyId}")]
        public async Task<VendorDetailsModel> GetVendorDetails(int companyId)
        {
            var res = await _dbContext.GetAsync<VendorDetailsModel>($@"Select CompanyID,VendorID,CompanyName,ContactPerson,ContactPersonNumber,CompanyAddress
                From Company 
				Where CompanyID=@CompanyID",new { CompanyID = companyId });
            return res;
        }

        [HttpGet("daily-request-checkin/{dailyPassRequestID}")]
        public async Task<bool> DailyRequestCheckin(int dailyPassRequestID)
        {
            DailyPassRequestTracking dailyPassRequestTracking = new DailyPassRequestTracking()
            {
                IsCheckOut = false,
                DailyPassRequestID = dailyPassRequestID
            };
            await _dbContext.SaveAsync(dailyPassRequestTracking);
            return true;
        }

        [HttpGet("daily-request-checkout/{dailyPassRequestID}")]
        public async Task<bool> DailyRequestCheckout(int dailyPassRequestID)
        {
            DailyPassRequestTracking dailyPassRequestTracking = new DailyPassRequestTracking()
            {
                IsCheckOut = true,
                DailyPassRequestID = dailyPassRequestID
            };
            await _dbContext.SaveAsync(dailyPassRequestTracking);
            return true;
        }

        [HttpGet("remove-material-media/{meterialMediaID}")]
        public async Task<bool> RemoveMaterialMedia(int meterialMediaID)
        {
            var mediaDetails = await _dbContext.GetAsync<RequestMeterialMedia>(meterialMediaID);
            if(mediaDetails != null)
            {
                await _media.DeleteExistingFileAsync(mediaDetails.MediaID);
                await _dbContext.DeleteAsync<RequestMeterialMedia>(meterialMediaID);
            }
            return true;
        }
    }
}
