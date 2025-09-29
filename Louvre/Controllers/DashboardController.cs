using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDbContext _dbContext;

        public DashboardController(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpPost("get-dashboard-data")]
        public async Task<DashboardDataModel> GetDashboardData(DashboardDataPostModel model)
        {
            DashboardDataModel result = new DashboardDataModel();

            DateTime fromDate, toDate;

            switch(model.Period)
            {
                case 1:
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                    break;
                case 2:
                    fromDate = DateTime.Now.AddDays(1).Date;
                    toDate = DateTime.Now.Date.AddDays(1).Date;
                    break;
                case 3:
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date.AddDays(7).Date;
                    break;
                case 4:
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date.AddMonths(1).Date;
                    break;
                default:
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date.AddYears(1).Date;
                    break;
            }
            var meterialSummary = new List<RequesterTrackingViewModel>();
            if (User.IsInRole("Monitor") || User.IsInRole("Super-Admin") || User.IsInRole("Administrator"))
            {
                meterialSummary = (await _dbContext.GetEnumerableAsync<RequesterTrackingViewModel>($@"Select R.RequestID, Case When TR.RequestVehicleTrackingID is null and TR.RequestVehicleTrackingID is null then 0 else 1 end as CheckedIn,
                Case When TR.RequestVehicleTrackingID is null then 0 else 1 end as CheckedOut
                From viRequest R 
                JOIN RequestVehicle D on D.RequestID=R.RequestID
                LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                Where R.StatusID>=4 and R.Date between @FromDate and @ToDate
                Order by R.Date", new { FromDate = fromDate, ToDate = toDate })).ToList();
            }
            else if(User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                meterialSummary = (await _dbContext.GetEnumerableAsync<RequesterTrackingViewModel>($@"Select R.RequestID, Case When TR.RequestVehicleTrackingID is null and TR.RequestVehicleTrackingID is null then 0 else 1 end as CheckedIn,
                Case When TR.RequestVehicleTrackingID is null then 0 else 1 end as CheckedOut
                From viRequest R 
                JOIN RequestVehicle D on D.RequestID=R.RequestID
                JOIN RequestTypeApprovalStage S on R.RequestTypeID=S.RequestTypeID
                LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                Where R.StatusID>=4 and  UserTypeID={CurrentUserTypeID} and R.Date between @FromDate and @ToDate
                Order by R.Date", new { FromDate = fromDate, ToDate = toDate })).ToList();
            }

            if (meterialSummary!=null)
            {
                result.SSFDashboard = new SSFDashboardModel()
                {
                    TotalRequester = meterialSummary.Count(),
                    CheckIn = meterialSummary.Where(l => l.CheckedIn == true && l.CheckedOut == false).Count(),
                    CheckOut = meterialSummary.Where(l => l.CheckedOut == true).Count(),
                    NotArrived = meterialSummary.Where(l => l.CheckedIn == false).Count()
                };
            }

            return result;
        }

       

    }
}
