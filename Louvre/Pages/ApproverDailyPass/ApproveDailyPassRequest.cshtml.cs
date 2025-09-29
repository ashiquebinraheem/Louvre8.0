using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Models.Enum;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages.Approver
{
    [Authorize(Roles = "Approver,Disposal")]
    [BindProperties]
    public class ApproveDailyPassRequestModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IErrorLogRepository _errorLogRepo;

        public ApproveDailyPassRequestModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _errorLogRepo = errorLogRepo;
        }

        public DailyPassRequestApprovalHeaderView Data { get; set; }
        public RequesterPostViewModel DriverData { get; set; }
        public VehicleListViewModel VehicleData { get; set; }
        public List<RequesterPostViewModel> Passengers { get; set; }
        public List<RequestMeterial> Meterials { get; set; }

        public List<MeterialFileViewModel> MeterialFiles { get; set; } = new List<MeterialFileViewModel>();

        public RequestApproval RequestApproval { get; set; }

        public List<DocumentPostViewModel> RequestDocuments { get; set; }
        public List<DocumentPostViewModel> RequesterDocuments { get; set; }
        public List<DocumentPostViewModel> VehicleDocuments { get; set; }
        public List<DocumentPostViewModel> DriverDocuments { get; set; }
        public List<DocumentPostViewModel> PassengerDocuments { get; set; }

        public int ApprovalCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Data = await _dbContext.GetAsync<DailyPassRequestApprovalHeaderView>($@"SELECT DailyPassRequestID,BranchName,SubBranchName,FromDate,ToDate,ModeName,RequestedLocationName,RequestedByID,RequestModeID,E.EmployeeName,E.Email,E.ContactNumber,C.CompanyName,D.DesignationName,BranchID,M.MeterialTypeName,R.ContainsExplosive, R.IsIn, R.IsDisposalRequired, R.MeterialTypeID,R.HostEmail,R.RequestNo,R.Narration
            FROM viDailyPassRequest R
            LEFT JOIN Employee E on E.EmployeeID = R.EmployeeID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
			LEFT JOIN RequestMeterialType M on M.MeterialTypeID=R.MeterialTypeID
            Where R.DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = id });

            var nextLevelUserTypeId = await _dbContext.GetAsync<int>($@"Select CASE WHEN U.UserNature=2 and R.IsDisposalRequired=0 then S1.UserTypeID else S.UserTypeID end
                from viDailyPassRequest R
                JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID and S.Stage=ISNULL(R.ApprovalStage,0)+1
			    JOIN UserTypes U on U.UserTypeID=S.UserTypeID
			    LEFT JOIN RequestTypeApprovalStage S1 on S1.RequestTypeID=R.RequestTypeID and S1.Stage=ISNULL(R.ApprovalStage,0)+2
                Where R.DailyPassRequestID=@DailyPassRequestID and IsRejected=0 and FromDate>=@FromDate", new { DailyPassRequestID = id, FromDate = CurrentClientTime.Date });

            if (nextLevelUserTypeId != CurrentUserTypeID)
                return Redirect("/home");

            DriverData = await _commonRepository.GetDailyPassDriverDetailsAsync(id);
            VehicleData = await _commonRepository.GetDailyPassVehicleDetailsAsync(id);
            Passengers = await _commonRepository.GetDailyPassPassengersAsync(id);
            Meterials = (await _dbContext.GetAllAsyncByFieldName<RequestMeterial>("DailyPassRequestID", id.ToString())).ToList();

            var Documents = await _commonRepository.GetDailyPassAllDocumentsAsync(Convert.ToInt32(Data.DailyPassRequestID));
            RequestDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Request).ToList();
            RequesterDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Requester).ToList();
            VehicleDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Vehicle).ToList();
            DriverDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Driver).ToList();
            PassengerDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Passenger).ToList();

            RequestApproval = await _dbContext.GetAsync<RequestApproval>($@"SELECT DailyPassRequestID, FromDate, LocationID,StorageLocationID
            FROM viDailyPassRequest
            Where DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = id });

            ApprovalCount = await _dbContext.GetAsync<int>($"SELECT  Count(RequestApprovalID) FROM  RequestApproval Where DailyPassRequestID = @DailyPassRequestID", new { DailyPassRequestID = id });

            MeterialFiles = (await _dbContext.GetEnumerableAsync<MeterialFileViewModel>($@"Select MM.MeterialMediaID,FileName,RequestID
                    From RequestMeterialMedia MM
                    JOIN Medias M on MM.MediaID=M.MediaID
                    Where MM.DailyPassRequestID=@DailyPassRequestID and MM.IsDeleted=0", new { DailyPassRequestID = id })).ToList();

            ViewData["PackingTypes"] = new SelectList((await _dbContext.GetAllAsync<PackingType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.PackingTypeID), Value = s.PackingTypeName }), "ID", "Value");
            var requestMode = await _dbContext.GetAsync<RequestMode>(Data.RequestModeID);
            ViewData["Locations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == requestMode.LocationTypeID).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");
            ViewData["StorageLocations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == (int)LocationTypes.Storage).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            BaseResponse result = new BaseResponse();
            try
            {
                RequestApproval.ApprovalStage = await GetCurrentApprovalStage();

                var pendingApprovalCount = await _dbContext.GetAsync<int>($@"SELECT  Count(S.Stage)
                FROM viDailyPassRequest R
                JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID and S.Stage>{RequestApproval.ApprovalStage}
                JOIN UserTypes U on U.UserTypeID=S.UserTypeID
                Where R.DailyPassRequestID=@DailyPassRequestID and 
                Case when U.UserNature=2 and R.IsDisposalRequired=0 then 0 else 1 end=1", new { DailyPassRequestID = RequestApproval.DailyPassRequestID });

                if (pendingApprovalCount > 0)
                    RequestApproval.NeedHigherLevelApproval = true;

                await _dbContext.SaveAsync(RequestApproval);

                if (pendingApprovalCount == 0)
                {
                    #region  Send Gatepass

                    var gatepassDatas = (await _dbContext.GetEnumerableAsync<GatePassViewModel>($@"Select R.DailyPassRequestID,E.QRCode,R.CompanyName,R.EmployeeName,DesignationName,
			        E.ContactNumber,PlateNo,VT.VehicleTypeName,VehicleSize,RegisterNo, LocationName,Coalesce(RQ.Email,P.Email1) as Email,
					ISNULL(B.GoogleLocation,'') as GoogleLocation,FromDate,ToDate
			        From viDailyPassRequest R
					JOIN Employee RQ on RQ.EmployeeID=R.EmployeeID
			        JOIN Vehicle V on V.VehicleID=R.VehicleID
			        JOIN Employee E on E.EmployeeID=R.DriverID
			        LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
			        LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
					LEFT JOIN viPersonalInfos P on P.UserID=R.RequestedByID
                    LEFT JOIN Branch B on B.BranchID=R.SubBranchID
                    Where R.DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = RequestApproval.DailyPassRequestID })).ToList();


                    var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                    var body = $@"<!DOCTYPE html>
					<html>
					<head>
					<title>Louvre - Gate Pass</title>
					<link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
					</head>
					<body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>

					<div style='width:600px;height:auto;background-color:#f5f5f5;margin: 0 auto;margin-top:50px;-webkit-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-moz-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-o-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);box-shadow: 0px 0px 20px rgba(0,0,0,0.1);border-radius: 10px;padding: 30px;'>
					
					<div style='width:100%;height:80px;background-color:#e4edf2;' align='center'>
						<img src='{url}/assets/img/brand.jpg' class='logo' style='width:200px;padding-top: 16px;'>
					</div>";

                    foreach (var item in gatepassDatas)
                    {

                        var image = _mediaRepository.GetQRImage(item.QRCode);
                        var msg = $"<img alt='Embedded Image'  width='200' src =\"{image}\">";

                        body += $@"
						<div style='background-color:none;width:100%;float: left;margin-top:10px;'>
						<center>{msg}</center>
						</div>

                        <div style='background-color:none;width:100%;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><center>{item.QRCode}</center></p>
						</div>

                        <div style='background-color:none;width:50%;float: left;'>
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>From Date:</span> {item.FromDate.Value.ToString("dd/MM/yyyy")}</p>
					    </div>

                        <div style='background-color:none;width:50%;float: left;'>
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>To Date:</span> {item.ToDate.Value.ToString("dd/MM/yyyy")}</p>
					    </div>
    
					    <div style='background-color:none;width:50%;float: left;'>
						    <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Location:</span> {item.LocationName}</p>
					    </div>
    
						<div style='background-color:none;width:50%;float: left;'>
						<p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Company Name:</span> {item.CompanyName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Name:</span> {item.EmployeeName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Mobile No:</span> {item.ContactNumber}</p>
						</div>
    
						<div style='background-color:none;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Designation:</span> {item.DesignationName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Register No:</span> {item.RegisterNo}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Vehicle Size:</span> {item.VehicleSize}</p>
						</div>

						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Plate No:</span> {item.PlateNo}</p>
						</div>

						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Type of Vehicle:</span> {item.VehicleTypeName}</p>
						</div>";
                    }

                    Passengers = await _commonRepository.GetPassengersAsync(RequestApproval.DailyPassRequestID.Value);

                    if (Passengers.Count > 0)
                    {

                        body += $@"<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;margin: 10px 0px;width: 100%;'>
									<tr>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Co-Passenger</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Designation</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Company</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Mobile No</th>
									</tr>";

                        foreach (var member in Passengers)
                        {
                            body += $@"<tr>
								<td style='border: 1px solid #ccc;'>{member.EmployeeName}</td>
								<td style='border: 1px solid #ccc;'>{member.DesignationName}</td>
								<td style='border: 1px solid #ccc;'>{member.CompanyName}</td>
                                <td style='border: 1px solid #ccc;'>{member.ContactNumber}</td>
							</tr>";
                        }
                        body += "</table>";

                    }

                    var meterials = await _commonRepository.GetDailyPassMeterialsAsync(RequestApproval.DailyPassRequestID.Value);

                    if (meterials.Count > 0)
                    {

                        body += $@"<center><table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;margin: 10px 10px;width: 100%;'>
									<tr>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>SlNo</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Meterial</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Quantity</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Unit</th>
									</tr>";

                        for (int i = 0, j = 1; i < meterials.Count(); i++, j++)
                        {
                            body += $@"<tr>
								<td style='border: 1px solid #ccc;'>{i + 1}</td>
								<td style='border: 1px solid #ccc;'>{meterials[i].Description}</td>
								<td style='border: 1px solid #ccc;'>{meterials[i].Quantity}</td>
								<td style='border: 1px solid #ccc;'>{meterials[i].PackingTypeName}</td>
							</tr>";
                        }
                        body += "</table></center>";

                    }

                    body += $@"
                          <div style='width:auto;height:80px;margin-top:10px;' align='center'>
                            <a href='{gatepassDatas[0].GoogleLocation}'>
						    <img src='{url}/map.png' class='logo' style='padding-top: 16px;width:100%'>
                            </a>
					    </div>  
					</html>";


                    body += $@"
                        <div style='width:auto;height:80px;margin-top:10px;' align='center'>
						    <img src='{url}/assets/img/full-brand.jpg' class='logo' style='padding-top: 16px;width:100%'>
					    </div>
                        </div></body>    
					</html>";

                    await _emailSender.SendEmailAsync(gatepassDatas[0].Email, "Gatepass", body);

                    #endregion

                    var request = await _dbContext.GetAsync<DailyPassRequest>(RequestApproval.DailyPassRequestID.Value);

                    #region Send email to host

                    //if (!string.IsNullOrEmpty(request.HostEmail))
                    //{
                    //    await _emailSender.SendHtmlEmailAsync(request.HostEmail, "Request Approved", body);
                    //}

                    #endregion

                    #region Explosive Item Mail

                    if (request.ContainsExplosive)
                    {

                        body = "";


                        foreach (var item in gatepassDatas)
                        {
                            body += $@"

                        <div style='background-color:none;width:100%;float: left;'>
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Barcode:</span> {item.QRCode}</p>
					    </div>

                        <div style='background-color:none;width:50%;float: left;'>
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>From Date:</span> {item.FromDate.Value.ToString("dd/MM/yyyy")}</p>
					    </div>

                        <div style='background-color:none;width:50%;float: left;'>
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>To Date:</span> {item.ToDate.Value.ToString("dd/MM/yyyy")}</p>
					    </div>
    
					    <div style='background-color:none;width:50%;float: left;'>
						    <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Location:</span> {item.LocationName}</p>
					    </div>
    
						<div style='background-color:none;width:50%;float: left;'>
						<p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Company Name:</span> {item.CompanyName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Name:</span> {item.EmployeeName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Mobile No:</span> {item.ContactNumber}</p>
						</div>
    
						<div style='background-color:none;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Designation:</span> {item.DesignationName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Register No:</span> {item.RegisterNo}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Vehicle Size:</span> {item.VehicleSize}</p>
						</div>

						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Plate No:</span> {item.PlateNo}</p>
						</div>

						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Type of Vehicle:</span> {item.VehicleTypeName}</p>
						</div>";
                        }

                        body += $@"<center><table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;margin: 10px 10px;width: 80%;'>
									<tr>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>SlNo</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Description</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Quantity</th>
									<th style='background-color: #B8DBFD;border: 1px solid #ccc'>Unit</th>
									</tr>";

                        for (int i = 0, j = 1; i < meterials.Count(); i++, j++)
                        {
                            body += $@"<tr>
								<td style='border: 1px solid #ccc;'>{i + 1}</td>
								<td style='border: 1px solid #ccc;'>{meterials[i].Description}</td>
								<td style='border: 1px solid #ccc;'>{meterials[i].Quantity}</td>
								<td style='border: 1px solid #ccc;'>{meterials[i].PackingTypeName}</td>
							</tr>";
                        }
                        body += "</table></center>";

                        var mailSettings = await _dbContext.GetAsync<MailSettings>(1);
                        await _emailSender.SendHtmlEmailAsync(mailSettings.MailTo, "Explosive Content", body);
                    }

                    #endregion

                    result.CreatSuccessResponse(102);
                }
                else
                {
                    #region Mail to appprovers

                    var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    await _commonRepository.SendNewDailyPassRequestMailToApprover(RequestApproval.DailyPassRequestID.Value, url);

                    #endregion

                    result.CreatSuccessResponse(103);
                }

            }
            catch (Exception err)
            {
                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
            }
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            BaseResponse result = new BaseResponse();
            try
            {
                RequestApproval.ApprovalStage = await GetCurrentApprovalStage();
                RequestApproval.NeedHigherLevelApproval = true;
                await _dbContext.SaveAsync(RequestApproval);
                result.CreatSuccessResponse(103);
            }
            catch (Exception err)
            {
                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
            }
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            BaseResponse result = new BaseResponse();
            try
            {
                RequestApproval.ApprovalStage = await GetCurrentApprovalStage();
                RequestApproval.IsRejected = true;
                await _dbContext.SaveAsync(RequestApproval);
                result.CreatSuccessResponse(104);


                #region  Send Reject Mail

                var data = await _dbContext.GetAsync<RequestRejectMailModel>($@"Select Coalesce(RQ.Email,P.Email1) as Email,RequestNo,Remarks,FromDate,ToDate
			        From viDailyPassRequest R
					JOIN Employee RQ on RQ.EmployeeID=R.EmployeeID
					LEFT JOIN viPersonalInfos P on P.UserID=R.RequestedByID
                    Where R.DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = RequestApproval.DailyPassRequestID });


                var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                var body = $@"<!DOCTYPE html>
					<html>
					<head>
					<title>Louvre - Request Rejected</title>
					<link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
					</head>
					<body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>

					<div style='width:600px;height:auto;background-color:#f5f5f5;margin: 0 auto;margin-top:50px;-webkit-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-moz-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-o-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);box-shadow: 0px 0px 20px rgba(0,0,0,0.1);border-radius: 10px;padding: 30px;'>
					
					<div style='width:100%;height:80px;background-color:#e4edf2;' align='center'>
						<img src='{url}/assets/img/brand.jpg' class='logo' style='width:200px;padding-top: 16px;'>
					</div>

                    <div style='background-color:none;width:100%;float: left;'>
						<p style='color: #666;padding-left: 0px; font-size: 14px;'>
                            Your request for gatepass Request No: {data.RequestNo} From {data.FromDate.Value.ToString("dd/MM/yyyy")} To {data.ToDate.Value.ToString("dd/MM/yyyy")} has been rejected for the following reason.<br/>
                            {data.Remarks}<br/>
                            Please resubmit the application.<br/><br/>
                            Thanks
                        </p>
					</div>";

                body += $@"
                        <div style='width:auto;height:80px;margin-top:10px;' align='center'>
						    <img src='{url}/assets/img/full-brand.jpg' class='logo' style='padding-top: 16px;width:100%'>
					    </div>
                        </div></body>    
					</html>";

                await _emailSender.SendEmailAsync(data.Email, "Request Rejected", body);

                #endregion


            }
            catch (Exception err)
            {
                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
            }
            return new JsonResult(result);
        }

        private async Task<int> GetCurrentApprovalStage()
        {
            return await _dbContext.GetAsync<int>($@"SELECT  S.Stage
                    FROM viDailyPassRequest R
                    LEFT JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID
                    Where R.DailyPassRequestID=@DailyPassRequestID and S.UserTypeID = @CurrentUserTypeID", new
            {
                DailyPassRequestID = RequestApproval.DailyPassRequestID,
                CurrentUserTypeID = CurrentUserTypeID
            });

        }
    }
}
