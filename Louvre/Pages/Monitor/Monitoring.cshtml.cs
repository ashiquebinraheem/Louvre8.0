using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEmailSender = Louvre.Shared.Repository.IEmailSender;

namespace Louvre.Pages
{
    
    [BindProperties]
    [Authorize(Roles = "Monitor")]
    public class MonitoringModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly ICommonRepository _commonRepository;
        private readonly IReflexionRepository _reflexion;
        private readonly IEmailSender _emailSender;

        public MonitoringModel(IDbContext dbContext, ICommonRepository commonRepository, IReflexionRepository reflexion,IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _commonRepository = commonRepository;
            _reflexion = reflexion;
            _emailSender = emailSender;
        }

        public List<CheckInCheckoutViewModel> Vehicles { get; set; }
        public List<DailyPassCheckInCheckoutViewModel> DailyPassVehicles { get; set; }

        public List<VisitorCheckInCheckoutViewModel> VisitVehicles { get; set; }

        public DateTime? Date { get; set; }
        public string? QRCode { get; set; }
        public int? RequestVehicleID { get; set; }
        public int? VisitRequestID { get; set; }
        public string? Remarks { get; set; }
        public int RequestID { get; set; }
        public int DailyPassRequestID { get; set; }

        public async Task OnGetAsync()
        {
            Date = DateTime.UtcNow.Date;
            Vehicles = await GetVehicles();
            //VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
    //        string HostEmail = await _dbContext.GetAsync<string>(
    //@"SELECT r.HostEmail 
    //  FROM RequestVehicle rv
    //  JOIN Request r ON rv.RequestID = r.RequestID
    //  WHERE rv.RequestVehicleID = @RequestVehicleID",
    //new { RequestVehicleID = RequestVehicleID });
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            Vehicles = await GetVehicles();
            //VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            return Page();
        }

        public async Task<IActionResult> OnPostCheckinAsync()
        {
            RequestVehicleTracking requestVehicleTracking = new RequestVehicleTracking()
            {
                IsCheckOut = false,
                RequestVehicleID = RequestVehicleID
            };
            await _dbContext.SaveAsync(requestVehicleTracking);

            Vehicles = await GetVehicles();
            VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            string HostEmail = await _dbContext.GetAsync<string>(
                @"SELECT r.HostEmail 
      FROM RequestVehicle rv
      JOIN Request r ON rv.RequestID = r.RequestID
      WHERE rv.RequestVehicleID = @RequestVehicleID",
                new { RequestVehicleID = RequestVehicleID });
            if (!string.IsNullOrEmpty(HostEmail) && !string.IsNullOrWhiteSpace(HostEmail))
            {
                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                var body = $@"<!DOCTYPE html>
				<html>
				<head>
				<title>Louvre - Request Delivered</title>
				<link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
				</head>
				<body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>

				<div style='width:600px;height:auto;background-color:#f5f5f5;margin: 0 auto;margin-top:50px;-webkit-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-moz-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-o-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);box-shadow: 0px 0px 20px rgba(0,0,0,0.1);border-radius: 10px;padding: 30px;'>
				
				<div style='width:100%;height:80px;background-color:#e4edf2;' align='center'>
					<img src='{url}/assets/img/brand.jpg' class='logo' style='width:200px;padding-top: 16px;'>
				</div>

    <div style='background-color:none;width:100%;float: left;'>
					<p style='color: #666;padding-left: 0px; font-size: 14px;'>
            Your Request has been delivered<br/>
            Thanks
        </p>
				</div>";

                body += $@"
        <div style='width:auto;height:80px;margin-top:10px;' align='center'>
					    <img src='{url}/assets/img/full-brand.jpg' class='logo' style='padding-top: 16px;width:100%'>
				    </div>
        </div></body>    
				</html>";
                await _emailSender.SendEmailAsync(HostEmail != null ? HostEmail.ToString() : "", "Request Delivered", body);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostLoadingBayVerifyAsync()
        {
            await _dbContext.ExecuteAsync($"update Request set IsLoadingBayVerified=1 where RequestID={RequestID}", null);

            Vehicles = await GetVehicles();
            //VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            return Page();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            RequestVehicleTracking requestVehicleTracking = new RequestVehicleTracking()
            {
                IsCheckOut = true,
                RequestVehicleID = RequestVehicleID
            };
            await _dbContext.SaveAsync(requestVehicleTracking);

            int checkoutCount = await _dbContext.GetAsync<int>($"Select Count(*) from RequestVehicleTracking Where RequestVehicleID=@RequestVehicleID and IsCheckOut=1", new { RequestVehicleID = RequestVehicleID });
            if(checkoutCount==1)
            {
                await _reflexion.PushDeliveryItem(RequestID);
            }

            Vehicles = await GetVehicles();
            //VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            return Page();
        }

        public async Task<IActionResult> OnPostForwardAsync()
        {
            var requestApproval = await _dbContext.GetAsync<RequestApproval>($@"SELECT TOP (1) *
                FROM RequestApproval
                WHere RequestID=@RequestID
                Order by RequestApprovalID desc", new { RequestID = RequestID });

            requestApproval.RequestApprovalID = null;
            requestApproval.IsReported = true;
            requestApproval.Remarks = Remarks;
            await _dbContext.SaveAsync(requestApproval);

            Vehicles = await GetVehicles();
            //VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            return Page();
        }

       

        private async Task<List<CheckInCheckoutViewModel>> GetVehicles()
        {
            string whereCondition = "";
            if (Date == null)
                whereCondition += $" and R.Date>@Date";
            else
                whereCondition += $" and R.Date=@Date";

            if (!string.IsNullOrEmpty(QRCode))
            {
                whereCondition += $" and QRCode=@QRCode";
            }

            var vehicles = (await _dbContext.GetEnumerableAsync<CheckInCheckoutViewModel>($@"Select R.RequestID,D.RequestVehicleID,QRCode,R.CompanyName,E.EmployeeName,DesignationName,
            ContactNumber,PlateNo,VT.VehicleTypeName,VehicleSize,RegisterNo,D.PassengerCount,R.Slot,R.LocationName,
            Case When T.RequestVehicleTrackingID is null then 1 else 0 end as NeedCheckin,
            Case When ISNULL(IsCheckOut,0)=0 then 1 else 0 end as NeedCheckout, IsLoadingBayVerified,
            Case When IsIn=0 and Isnull(IsLoadingBayVerified,0)=0 then 1 else 0 end as NeedLoadingBayVerify
            From viRequest R
            JOIN RequestVehicle D on D.RequestID=R.RequestID
            JOIN Vehicle V on V.VehicleID=D.VehicleID
            JOIN Employee E on E.EmployeeID=D.EmployeeID
            LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
            LEFT JOIN Company C on C.CompanyID=E.CompanyID
            LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
            LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Where AddedBy=@CurrentUserID Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
            LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
            Where R.StatusID in(4,6) {whereCondition} and ISNULL(IsCheckOut,0)=0
            Order by R.Date", new { Date = Date == null?DateTime.UtcNow.Date:Date, QRCode= QRCode, CurrentUserID= CurrentUserID })).ToList();

            foreach (var item in vehicles)
            {
                item.Meterials = await _commonRepository.GetMeterialsAsync(item.RequestID);
                item.Passengers = await _commonRepository.GetPassengersAsync(item.RequestID);

                item.Meterials = item.Meterials ?? new List<MeterialViewModel>();
                item.Passengers = item.Passengers ?? new List<RequesterPostViewModel>();
            }

            return vehicles;
        }

        private async Task<List<DailyPassCheckInCheckoutViewModel>> GetDailyRequestVehicles()
        {
            string whereCondition = "";
            if (Date == null)
                whereCondition += $" and R.FromDate<=@Date and R.ToDate>=@Date";
            else
                whereCondition += $" and R.FromDate<=@Date and R.ToDate>=@Date";

            if (!string.IsNullOrEmpty(QRCode))
            {
                whereCondition += $" and QRCode=@QRCode";
            }

            var vehicles = (await _dbContext.GetEnumerableAsync<DailyPassCheckInCheckoutViewModel>($@"Select R.DailyPassRequestID,QRCode,R.CompanyName,E.EmployeeName,DesignationName,
            ContactNumber,PlateNo,VT.VehicleTypeName,VehicleSize,RegisterNo,R.LocationName,
            Case When T.TrackingID is null then 1 else 0 end as NeedCheckin,
            Case When ISNULL(IsCheckOut,0)=0 then 1 else 0 end as NeedCheckout, IsLoadingBayVerified,
            Case When IsIn=0 and Isnull(IsLoadingBayVerified,0)=0 then 1 else 0 end as NeedLoadingBayVerify,
            R.FromDate,R.ToDate
            From viDailyPassRequest R
            JOIN Vehicle V on V.VehicleID=R.VehicleID
            JOIN Employee E on E.EmployeeID=R.DriverID
            LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
            LEFT JOIN Company C on C.CompanyID=E.CompanyID
            LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
            LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where AddedBy=@CurrentUserID and Convert(date,AddedOn)=Convert(date,GETDATE()) Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
            LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
            Where R.StatusID=4 {whereCondition} and ISNULL(IsCheckOut,0)=0
            Order by R.FromDate", new { Date = Date ==null? DateTime.UtcNow.Date: Date , QRCode, CurrentUserID })).ToList();

            foreach (var item in vehicles)
            {
                item.Meterials = await _commonRepository.GetDailyPassMeterialsAsync(item.DailyPassRequestID);
                item.Passengers = await _commonRepository.GetDailyPassPassengersAsync(item.DailyPassRequestID);

                item.Meterials = item.Meterials ?? new List<MeterialViewModel>();
                item.Passengers = item.Passengers ?? new List<RequesterPostViewModel>();
            }

            return vehicles;
        }

        public async Task<IActionResult> OnPostVisitorCheckinAsync()
        {
            VisitorTracking tracking = new VisitorTracking()
            {
                IsCheckOut = false,
                VisitRequestID = VisitRequestID
            };
            await _dbContext.SaveAsync(tracking);

            Vehicles = await GetVehicles();
            VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            return Page();
        }

        public async Task<IActionResult> OnPostVisitorCheckoutAsync()
        {
            VisitorTracking tracking = new VisitorTracking()
            {
                IsCheckOut = true,
                VisitRequestID = VisitRequestID
            };
            await _dbContext.SaveAsync(tracking);

            Vehicles = await GetVehicles();
            VisitVehicles = await GetVisitorVehicles();
            DailyPassVehicles = await GetDailyRequestVehicles();
            return Page();
        }

        private async Task<List<VisitorCheckInCheckoutViewModel>> GetVisitorVehicles()
        {
            string whereCondition = "";
            if (Date == null)
                whereCondition += $" and MeetingDate>@Date";
            else
                whereCondition += $" and CONVERT(date, MeetingDate)=@Date";

            if (!string.IsNullOrEmpty(QRCode))
            {
                whereCondition += $" and QRCode=@QRCode";
            }

            return (await _dbContext.GetEnumerableAsync<VisitorCheckInCheckoutViewModel>($@"Select R.VisitRequestID, Requester, DepartmentName, AreaName, 
            PurposeName, EmployeeName,MeetingDate, DurationName, Remark, VehicleID, PlateNo, RegisterNo, IsParkingRequired, QRCode,
            Case When T.VisitorTrackingID is null then 1 else 0 end as NeedCheckin,
            Case When ISNULL(IsCheckOut,0)=0 then 1 else 0 end as NeedCheckout
            From viVisitRequest R
            LEFT JOIN (Select VisitRequestID,Max(VisitorTrackingID) as VisitorTrackingID From VisitorTracking Where AddedBy={CurrentUserID} Group by VisitRequestID) as TR on TR.VisitRequestID=R.VisitRequestID
            LEFT JOIN VisitorTracking T on T.VisitorTrackingID=TR.VisitorTrackingID
            Where IsApproved=1 {whereCondition} and ISNULL(IsCheckOut,0)=0
            Order by R.MeetingDate", new { Date = Date == null ? DateTime.UtcNow.Date : Date, QRCode, CurrentUserID })).ToList();
        }

    }
}
