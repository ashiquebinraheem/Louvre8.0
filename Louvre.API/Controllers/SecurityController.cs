using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Models.Enum;
using Louvre.Shared.Repository;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Progbiz.API.Controllers
{
    [Route("api/security")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Monitor")]
    public class SecurityController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly ICommonRepository _commonRepository;

        public SecurityController(IDbContext dbContext, ICommonRepository commonRepository)
        {
            _dbContext = dbContext;
            _commonRepository = commonRepository;
        }

        [HttpGet("get-data")]
        public async Task<SecurityHomeDetail> GetHomeData()
        {
            SecurityHomeDetail result = new SecurityHomeDetail();

            var meterialSummary = (await _dbContext.GetEnumerableAsync<RequesterTrackingViewModel>($@"Select R.RequestID, Case When T.RequestVehicleTrackingID is null and CO.RequestVehicleTrackingID is null then 0 else 1 end as CheckedIn,Case When CO.RequestVehicleTrackingID is null then 0 else 1 end as CheckedOut
                From viRequest R 
                JOIN RequestVehicle D on D.RequestID=R.RequestID
                LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Where AddedBy={CurrentUserID} Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
			    LEFT JOIN RequestVehicleTracking CO on CO.RequestVehicleID=D.RequestVehicleID and CO.IsCheckOut=1
                Where R.StatusID>=4 and R.Date=@Date

                UNION

                Select R.DailyPassRequestID, Case When T.RequestVehicleTrackingID is null and CO.RequestVehicleTrackingID is null then 0 else 1 end as CheckedIn,Case When CO.RequestVehicleTrackingID is null then 0 else 1 end as CheckedOut
                From viDailyPassRequest R 
                LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Where AddedBy={CurrentUserID} Group by RequestVehicleID) as TR on TR.RequestVehicleID=R.VehicleID
                LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
			    LEFT JOIN RequestVehicleTracking CO on CO.RequestVehicleID=R.VehicleID and CO.IsCheckOut=1
                Where R.StatusID>=4 and R.FromDate<=@Date and R.ToDate>=@Date", new { Date = CurrentClientTime.Date })).ToList();

            result.TotalVisitCount = meterialSummary.Count();
            result.CheckoutCount = meterialSummary.Where(l => l.CheckedOut == true).Count();
            result.ActiveVisitorCount = meterialSummary.Where(l => l.CheckedIn == true && l.CheckedOut == false).Count();
            result.Name = User.Identity.Name;
            return result;
        }

        [HttpPost("scan-qrcode")]
        public async Task<QRScanResponseViewModel> InsertVisit(QRScanResponsPostModel qrCode)
        {
            QRScanResponseViewModel response = new QRScanResponseViewModel();

            var vehicle = (await _dbContext.GetAsync<CheckInCheckoutViewModel>($@"Select R.RequestID,D.RequestVehicleID,QRCode,R.CompanyName,E.EmployeeName,DesignationName,
            ContactNumber,PlateNo,VT.VehicleTypeName,VehicleSize,Case when IsIn=0 then 'Exit' else 'Entry' end as RegisterNo,D.PassengerCount,R.Slot,R.LocationName,
            Case When T.RequestVehicleTrackingID is null then 1 else 0 end as NeedCheckin,
            Case When ISNULL(IsCheckOut,0)=0 then 1 else 0 end as NeedCheckout,
			R.BranchName,R.SubBranchName,ContainsExplosive,RequestTypeName, AllotedDate, IsLoadingBayVerified,
            Case When IsIn=0 and Isnull(IsLoadingBayVerified,0)=0 then 1 else 0 end as NeedLoadingBayVerify
            From viRequest R
            JOIN RequestVehicle D on D.RequestID=R.RequestID
            JOIN Vehicle V on V.VehicleID=D.VehicleID
            JOIN Employee E on E.EmployeeID=D.EmployeeID
            LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
            LEFT JOIN Company C on C.CompanyID=E.CompanyID
            LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
            LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Where AddedBy=@AddedBy Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
            LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
            LEFT JOIN RequestType RT on R.RequestTypeID=RT.RequestTypeID
            Where R.StatusID in(4,6)  and ISNULL(IsCheckOut,0)=0 and AllotedDate>=@AllotedDate
            and QRCode =@QRCode
            Order by R.Date", new { QRCode = qrCode.QRCode, AllotedDate= CurrentClientTime.Date,AddedBy= CurrentUserID }));

            if (vehicle == null)
            {
                vehicle = await _dbContext.GetAsync<CheckInCheckoutViewModel>($@"Select R.DailyPassRequestID*-1 RequestID,R.VehicleID RequestVehicleID,QRCode,R.CompanyName,E.EmployeeName,DesignationName,
                    ContactNumber,PlateNo,VT.VehicleTypeName,VehicleSize,Case when IsIn=0 then 'Exit' else 'Entry' end as RegisterNo,R.PassengersCount PassengerCount,Convert(varchar,R.FromDate,103)+' to '+Convert(varchar,R.ToDate,103) Slot,R.LocationName,
                    Case When T.TrackingID is null then 1 else 0 end as NeedCheckin,
                    Case When ISNULL(IsCheckOut,0)=0 then 1 else 0 end as NeedCheckout,
			        R.BranchName,R.SubBranchName,ContainsExplosive,RequestTypeName, GETDATE() AllotedDate, IsLoadingBayVerified,
                    Case When IsIn=0 and Isnull(IsLoadingBayVerified,0)=0 then 1 else 0 end as NeedLoadingBayVerify
                    From viDailyPassRequest R
                    JOIN Employee E on E.EmployeeID=R.DriverID
                    LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
                    LEFT JOIN Company C on C.CompanyID=E.CompanyID
			        LEFT JOIN Vehicle V on V.VehicleID=R.VehicleID	
                    LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
                    LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where AddedBy=@CurrentUserID and Convert(date,AddedOn)=Convert(date,GETDATE()) Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                    LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                    LEFT JOIN RequestType RT on R.RequestTypeID=RT.RequestTypeID
			        Where R.StatusID in(4,6)  and ISNULL(IsCheckOut,0)=0 
			        and FromDate<=@CurrentDate and ToDate>=@CurrentDate and QRCode =@QRCode", new { QRCode =qrCode.QRCode,CurrentDate= CurrentClientTime.Date, CurrentUserID= CurrentUserID });
                if (vehicle == null)
                {
                    response.CreateFailureResponse("QR code not found");
                    return response;
                }
                response.VisitID = vehicle.RequestID;
            }
            else
            {
                response.VisitID = vehicle.RequestVehicleID;
                if (vehicle.AllotedDate != CurrentClientTime.Date)
                {
                    response.CreateFailureResponse($"Slot alloted on {vehicle.AllotedDate.Value.Date}");
                    return response;
                }
            }

            vehicle.Meterials = await _commonRepository.GetMeterialsAsync(vehicle.RequestID);
            vehicle.Passengers = await _commonRepository.GetPassengersAsync(vehicle.RequestID);

            var Documents = await _commonRepository.GetAllDocumentsAsync(Convert.ToInt32(vehicle.RequestID));
            response.Documents = Documents;//.Where(s => s.DocumentOf == (int)DocumentOf.Requester).ToList();

            vehicle.Meterials ??= new List<MeterialViewModel>();
            vehicle.Passengers ??= new List<RequesterPostViewModel>();

            response.Type = 1;
            response.Meterial = vehicle;

            return response;
        }

        [HttpPost("checkin")]
        public async Task<APIBaseResponse> OnPostCheckinAsync(CheckinPostModel model)
        {
            if (model.VisitType == 1)
            {
                if (model.VisitID > 0)
                {
                    RequestVehicleTracking requestVehicleTracking = new RequestVehicleTracking()
                    {
                        IsCheckOut = false,
                        RequestVehicleID = model.VisitID
                    };
                    await _dbContext.SaveAsync(requestVehicleTracking);
                }
                else
                {
                    DailyPassRequestTracking requestVehicleTracking = new DailyPassRequestTracking()
                    {
                        IsCheckOut = false,
                        DailyPassRequestID =-1* model.VisitID,
                        AddedBy=CurrentUserID,
                        AddedOn=DateTime.UtcNow
                    };
                    await _dbContext.SaveAsync(requestVehicleTracking);
                }
            }
            else
            {
                VisitorTracking tracking = new VisitorTracking()
                {
                    IsCheckOut = false,
                    VisitRequestID = model.VisitID
                };
                await _dbContext.SaveAsync(tracking);
            }

            return new APIBaseResponse() { Message = "Successfully Checked in" };
        }

        [HttpPost("verify")]
        public async Task<APIBaseResponse> OnPostVerifyAsync(LoadingBayVerifyPostModel model)
        {
            await _dbContext.ExecuteAsync($"update Request set IsLoadingBayVerified=1 where RequestID={model.RequestID}", null);

            return new APIBaseResponse() { Message = "Successfully Verified" };
        }

        [HttpPost("checkout")]
        public async Task<APIBaseResponse> OnPostCheckoutAsync(CheckinPostModel model)
        {
            if (model.VisitType == 1)
            {
                if (model.VisitID > 0)
                {
                    RequestVehicleTracking requestVehicleTracking = new RequestVehicleTracking()
                    {
                        IsCheckOut = true,
                        RequestVehicleID = model.VisitID
                    };
                    await _dbContext.SaveAsync(requestVehicleTracking);
                }
                else
                {
                    DailyPassRequestTracking requestVehicleTracking = new DailyPassRequestTracking()
                    {
                        IsCheckOut = true,
                        DailyPassRequestID = -1 * model.VisitID,
                        AddedBy = CurrentUserID,
                        AddedOn = DateTime.UtcNow
                    };
                    await _dbContext.SaveAsync(requestVehicleTracking);
                }
            }
            else
            {
                VisitorTracking tracking = new VisitorTracking()
                {
                    IsCheckOut = true,
                    VisitRequestID = model.VisitID
                };
                await _dbContext.SaveAsync(tracking);
            }

            return new APIBaseResponse() { Message = "Successfully Checked out" };
        }

    }
}
