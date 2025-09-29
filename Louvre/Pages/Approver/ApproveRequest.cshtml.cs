using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Approver,Disposal")]
    [BindProperties]
    public class ApproveRequestModel : BasePageModel   //  -- Added By Abdul Razack for Denial of Service via Unrestricted File Upload Size and Rate
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IErrorLogRepository _errorLogRepo;

        private readonly string[] allowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".pdf" };
        private const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        private const int maxFilesPerHour = 20;

        // Tracks per-user uploads for rate limiting
        private static readonly Dictionary<int, List<DateTime>> UserUploadHistory = new Dictionary<int, List<DateTime>>();

        public ApproveRequestModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository,
                                   IMediaRepository mediaRepository, IEmailSender emailSender, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _errorLogRepo = errorLogRepo;
        }

        #region Properties
        public RequestApprovalHeaderView Data { get; set; }
        public RequesterPostViewModel DriverData { get; set; }
        public VehicleListViewModel VehicleData { get; set; }
        public List<RequesterPostViewModel> Passengers { get; set; }
        public List<RequestMeterial> Meterials { get; set; }
        public List<MeterialFileViewModel> MeterialFiles { get; set; } = new List<MeterialFileViewModel>();
        public RequestApproval RequestApproval { get; set; }
        public List<DocumentPostViewModel> RequestDocuments { get; set; } = new List<DocumentPostViewModel>();
        public List<DocumentPostViewModel> RequesterDocuments { get; set; } = new List<DocumentPostViewModel>();
        public List<DocumentPostViewModel> VehicleDocuments { get; set; } = new List<DocumentPostViewModel>();
        public List<DocumentPostViewModel> DriverDocuments { get; set; } = new List<DocumentPostViewModel>();
        public List<DocumentPostViewModel> PassengerDocuments { get; set; } = new List<DocumentPostViewModel>();
        public List<IFormFile>? UploadedFiles { get; set; } = new List<IFormFile>();
        public List<RequestItemModel> Spares { get; set; } = new List<RequestItemModel>();
        public List<RequestItemModel> Assets { get; set; } = new List<RequestItemModel>();
        public List<RequestItemModel> Consumables { get; set; } = new List<RequestItemModel>();
        public int ApprovalCount { get; set; }
        #endregion

        #region File Validation Helper
        private void ValidateFile(IFormFile file, string fileName)
        {
            if (file == null)
                return;

            if (file.Length > maxFileSize)
                throw new Exception(fileName + " exceeds maximum allowed size of 10 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                throw new Exception(fileName + " has invalid file type. Allowed types: " + string.Join(", ", allowedExtensions));
        }

        private bool CanUploadFile(int userId)
        {
            lock (UserUploadHistory)
            {
                if (!UserUploadHistory.ContainsKey(userId))
                    UserUploadHistory[userId] = new List<DateTime>();

                // Remove timestamps older than 1 hour
                UserUploadHistory[userId] = UserUploadHistory[userId].Where(t => t > DateTime.UtcNow.AddHours(-1)).ToList();

                // Check limit
                if (UserUploadHistory[userId].Count >= maxFilesPerHour)
                    return false;

                UserUploadHistory[userId].Add(DateTime.UtcNow);
                return true;
            }
        }
        #endregion

        #region OnGetAsync
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Data = await _dbContext.GetAsync<RequestApprovalHeaderView>(
                @"SELECT RequestID,RequestedDate,BranchName,SubBranchName,RequestedSlot,ModeName,RequestedLocationName,
                  RequestedByID,RequestModeID,E.EmployeeName,E.Email,E.ContactNumber,C.CompanyName,D.DesignationName,
                  BranchID,SlotID,M.MeterialTypeName,R.ContainsExplosive,R.ContainsCarryItem,R.IsIn,R.IsDisposalRequired,
                  R.MeterialTypeID,R.HostEmail,R.RequestNo,R.Narration,IsProjectAsset,PONumber,PODate,DeliveryDate,POOwnerID
                  FROM viRequest R
                  LEFT JOIN Employee E on E.EmployeeID = R.EmployeeID
                  LEFT JOIN Company C on C.CompanyID = E.CompanyID
                  LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
                  LEFT JOIN RequestMeterialType M on M.MeterialTypeID=R.MeterialTypeID
                  Where R.RequestID=@RequestID", new { RequestID = id });

            string LocationName = await _dbContext.GetAsync<string>(
                @"SELECT L.LocationName FROM Request R Left join Location L on L.LocationID = R.StorageLocationID WHERE RequestID = @RequestID",
                new { RequestID = id });

            Data.LocationName = LocationName ?? "";

            var nextLevelUserTypeId = await _dbContext.GetAsync<int>(
                @"Select CASE WHEN U.UserNature=2 and R.IsDisposalRequired=0 then S1.UserTypeID else S.UserTypeID end
                  from viRequest R
                  JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID and S.Stage=ISNULL(R.ApprovalStage,0)+1
                  JOIN UserTypes U on U.UserTypeID=S.UserTypeID
                  LEFT JOIN RequestTypeApprovalStage S1 on S1.RequestTypeID=R.RequestTypeID and S1.Stage=ISNULL(R.ApprovalStage,0)+2
                  Where R.RequestID=@RequestID and IsRejected=0 and Date>=@Date",
                new { RequestID = id, Date = CurrentClientTime.Date });

            if (nextLevelUserTypeId != CurrentUserTypeID)
                return Redirect("/home");

            DriverData = await _commonRepository.GetDriverDetailsAsync(id);
            VehicleData = await _commonRepository.GetVehicleDetailsAsync(id);
            Passengers = await _commonRepository.GetPassengersAsync(id);

            if (Data.IsProjectAsset)
            {
                var meterials = await _dbContext.GetEnumerableAsync<RequestItemModel>(
                    @"Select R.*,I.Name,I.Code,I.PurchaseUnit as Unit,I.IsExpirable,I.Type
                      From RequestItem R
                      JOIN ItemMaster I on I.ItemID=R.ItemID
                      Where R.RequestID=@RequestID and R.IsDeleted=0", new { RequestID = id });

                Spares = new List<RequestItemModel>(meterials.Where(s => s.Type == "Spares"));
                Assets = new List<RequestItemModel>(meterials.Where(s => s.Type == "Assets"));
                Consumables = new List<RequestItemModel>(meterials.Where(s => s.Type == "Consumables"));
            }
            else
            {
                Meterials = new List<RequestMeterial>(
                    await _dbContext.GetAllAsyncByFieldName<RequestMeterial>("RequestID", id.ToString())
                );

                var metFiles = await _dbContext.GetEnumerableAsync<MeterialFileViewModel>(
                    @"Select MM.MeterialMediaID,FileName,RequestID
                      From RequestMeterialMedia MM
                      JOIN Medias M on MM.MediaID=M.MediaID
                      Where MM.RequestID=@RequestID and MM.IsDeleted=0", new { RequestID = id });

                MeterialFiles = new List<MeterialFileViewModel>();
                foreach (var m in metFiles)
                {
                    MeterialFiles.Add(new MeterialFileViewModel
                    {
                        MediaID = m.MeterialMediaID,
                        FileName = m.FileName,
                        RequestID = m.RequestID
                    });
                }
            }

            var documents = await _commonRepository.GetAllDocumentsAsync(Data.RequestID);
            RequestDocuments = documents.Where(s => s.DocumentOf == (int)DocumentOf.Request).ToList();
            RequesterDocuments = documents.Where(s => s.DocumentOf == (int)DocumentOf.Requester).ToList();
            VehicleDocuments = documents.Where(s => s.DocumentOf == (int)DocumentOf.Vehicle).ToList();
            DriverDocuments = documents.Where(s => s.DocumentOf == (int)DocumentOf.Driver).ToList();
            PassengerDocuments = documents.Where(s => s.DocumentOf == (int)DocumentOf.Passenger).ToList();

            RequestApproval = await _dbContext.GetAsync<RequestApproval>(
                @"SELECT RequestID, Date, SlotID, LocationID,StorageLocationID
                  FROM viRequest
                  Where RequestID=@RequestID", new { RequestID = id });

            ApprovalCount = await _dbContext.GetAsync<int>(
                "SELECT Count(RequestApprovalID) FROM RequestApproval Where RequestID = @RequestID",
                new { RequestID = id });

            ViewData["PackingTypes"] = new SelectList(
                (await _dbContext.GetAllAsync<PackingType>()).ToList().Select(s => new IdnValuePair
                {
                    ID = Convert.ToInt32(s.PackingTypeID),
                    Value = s.PackingTypeName
                }), "ID", "Value");

            var requestMode = await _dbContext.GetAsync<RequestMode>(Data.RequestModeID);
            ViewData["Locations"] = new SelectList(
                (await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == requestMode.LocationTypeID)
                .Select(s => new IdnValuePair { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }),
                "ID", "Value");

            ViewData["StorageLocations"] = new SelectList(
                (await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == (int)LocationTypes.Storage)
                .Select(s => new IdnValuePair { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }),
                "ID", "Value");

            ViewData["Slots"] = new SelectList((await GetSlots()).ToList(), "ID", "Value");
            ViewData["POOwners"] = new SelectList((await _dbContext.GetAllAsync<POOwner>()).ToList(), "POOwnerID", "StaffName");

            return Page();
        }
        #endregion

        #region File Upload Handling
        //  //  -- Added By Abdul Razack for Denial of Service via Unrestricted File Upload Size and Rate
        private async Task SaveFilesAsync(int requestId, List<MeterialFileViewModel> files, List<IFormFile> uploadedFiles)
        {
            if (!CanUploadFile(CurrentUserID))
            {
                _errorLogRepo.Log("User " + CurrentUserID + " exceeded upload limit.");
                throw new Exception("Upload limit exceeded. Please try again later.");
            }

            foreach (var fileModel in files)
            {
                if (fileModel.File != null)
                {
                    ValidateFile(fileModel.File, fileModel.FileName);
                    var result = await _mediaRepository.SaveMedia(fileModel.MediaID, fileModel.File, "meterial_files",
                                                                   requestId + "_" + fileModel.FileName, null);

                    if (!result.IsSuccess)
                        _errorLogRepo.Log("Failed to save file " + fileModel.FileName + " for request " + requestId);

                    fileModel.MediaID = result.IsSuccess ? result.MediaID : (int?)null;
                }
            }
        }
        #endregion


        public async Task<IActionResult> OnPostApproveAsync()
        {
            BaseResponse result = new BaseResponse();
            try
            {
                RequestApproval.ApprovalStage = await GetCurrentApprovalStage();

                var pendingApprovalCount = await _dbContext.GetAsync<int>($@"SELECT  Count(S.Stage)
                FROM viRequest R
                JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID and S.Stage>@Stage
                JOIN UserTypes U on U.UserTypeID=S.UserTypeID
                Where R.RequestID=@RequestID and Case when U.UserNature=2 and R.IsDisposalRequired=0 then 0 else 1 end=1",
                new { RequestID = RequestApproval.RequestID, Stage = RequestApproval.ApprovalStage });

                if (pendingApprovalCount > 0)
                    RequestApproval.NeedHigherLevelApproval = true;

                await _dbContext.SaveAsync(RequestApproval);

                if (pendingApprovalCount == 0)
                {
                    #region  Send Gatepass

                    var gatepassDatas = (await _dbContext.GetEnumerableAsync<GatePassViewModel>($@"Select R.RequestID,D.RequestVehicleID,E.QRCode,R.CompanyName,R.EmployeeName,DesignationName,
			        E.ContactNumber,PlateNo,VT.VehicleTypeName,VehicleSize,RegisterNo, Slot, LocationName,Coalesce(RQ.Email,P.Email1) as Email,
					ISNULL(B.GoogleLocation,'') as GoogleLocation
			        From viRequest R
					JOIN Employee RQ on RQ.EmployeeID=R.EmployeeID
			        JOIN RequestVehicle D on D.RequestID=R.RequestID
			        JOIN Vehicle V on V.VehicleID=D.VehicleID
			        JOIN Employee E on E.EmployeeID=D.EmployeeID
			        LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
			        LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
					LEFT JOIN viPersonalInfos P on P.UserID=R.RequestedByID
                    LEFT JOIN Branch B on B.BranchID=R.SubBranchID
                    Where R.RequestID=@RequestID", new { RequestID = RequestApproval.RequestID })).ToList();


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
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Slot:</span> {item.Slot}</p>
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

                    Passengers = await _commonRepository.GetPassengersAsync(RequestApproval.RequestID.Value);

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

                    var meterials = await _commonRepository.GetMeterialsAsync(RequestApproval.RequestID.Value);

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

                    var request = await _dbContext.GetAsync<Request>(RequestApproval.RequestID.Value);

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
						    <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Slot:</span> {item.Slot}</p>
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
                    await _commonRepository.SendNewRequestMailToApprover(RequestApproval.RequestID.Value, url);

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

                var data = await _dbContext.GetAsync<RequestRejectMailModel>($@"Select Coalesce(RQ.Email,P.Email1) as Email,RequestNo,Remarks,SLot
			        From viRequest R
					JOIN Employee RQ on RQ.EmployeeID=R.EmployeeID
					LEFT JOIN viPersonalInfos P on P.UserID=R.RequestedByID
                    Where R.RequestID=@RequestID", new { RequestID = RequestApproval.RequestID });


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
                            Your request for gatepass Request No: {data.RequestNo} on {data.Slot} has been rejected for the following reason.<br/>
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

        public async Task<IActionResult> OnPostLoadSlotAsync()
        {
            return new JsonResult(await GetSlots());
        }

        private async Task<IEnumerable<IdnValuePair>> GetSlots()
        {
            return await _dbContext.GetEnumerableAsync<IdnValuePair>(
                @"Select SlotID as ID,SlotName as Value
                  From viSlot
                  Where Date = @Date and BranchID = " + Data.BranchID + " and AvailableCount>=Case When SlotID=" + Data.SlotID + " then 0 else 1 end",
                RequestApproval);
        }

        private async Task<int> GetCurrentApprovalStage()
        {
            return await _dbContext.GetAsync<int>(
                @"SELECT S.Stage
                  FROM viRequest R
                  LEFT JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID
                  Where R.RequestID=@RequestID and S.UserTypeID = @UserTypeID",
                new { RequestID = RequestApproval.RequestID, UserTypeID = CurrentUserTypeID });
        }
    }
}
