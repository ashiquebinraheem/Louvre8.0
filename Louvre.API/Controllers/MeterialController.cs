using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Progbiz.DapperEntity;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Louvre.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Louvre.Shared.Core;
using System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Progbiz.API.Controllers
{
    [Route("api/meterial")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MeterialController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public MeterialController(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        //[HttpGet("get-data")]
        //public async Task<MeterialHomeDetail> GetHomeData()
        //{
        //    MeterialHomeDetail result = new MeterialHomeDetail();

        //    var personalInfo = await _commonRepository.GetProfileInfo(CurrentUserID);

        //    result.QRCode = _mediaRepository.GetQRImage(personalInfo.QRCode);
        //    result.Name = User.Identity.Name;
        //    result.EmailAddress = personalInfo.EmailAddress;
        //    result.MobileNumber = personalInfo.MobileNumber;
        //    result.CompanyName = personalInfo.CompanyName;

        //    var requestCount = await _dbContext.ExecuteScalarAsync<int>($@"SELECT COUNT(RequestID) FROM viRequest WHERE (RequestedByID ={CurrentUserID}) and Date>=@CurrentClientTime", new { CurrentClientTime });
        //    var visitrequestCount = await _dbContext.ExecuteScalarAsync<int>($@"Select COUNT(VisitRequestID) From viVisitRequest WHERE (RequestedByID ={CurrentUserID}) and MeetingDate>=CurrentClientTime", new { CurrentClientTime });
        //    result.TotalVisitCount = requestCount + visitrequestCount;
        //    var summary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT StatusID as ID,Count(StatusID) as Value FROM viRequest WHERE (RequestedByID = {CurrentUserID}) and Date>=@CurrentClientTime Group by StatusID
        //                UNION
        //                SELECT StatusID as ID,Count(StatusID) as Value FROM viVisitRequest WHERE (RequestedByID = {CurrentUserID}) and MeetingDate>=@CurrentClientTime Group by StatusID", new { CurrentClientTime })).ToList();

        //    result.PendingCount = Convert.ToInt32(summary.Where(s => s.ID == (int)RequestStatus.Pending).Select(s => s.Value).FirstOrDefault() ?? "0");
        //    result.ApprovedVisitCount = Convert.ToInt32(summary.Where(s => s.ID == (int)RequestStatus.Accepted).Select(s => s.Value).FirstOrDefault() ?? "0");
        //    result.RejectedCount = Convert.ToInt32(summary.Where(s => s.ID == (int)RequestStatus.Rejected).Select(s => s.Value).FirstOrDefault() ?? "0");


        //    var branches = await _dbContext.GetAllAsync<Branch>();
        //    foreach (var branch in branches.Where(b => b.ParentBranchID == null))
        //    {
        //        var mainBranch = new MainBranchModel();

        //        foreach (var subBranch in branches.Where(s => s.ParentBranchID == branch.BranchID))
        //        {
        //            mainBranch.SubBranches.Add(new SubBranchModel() { BranchID = subBranch.BranchID.Value, BranchName = subBranch.BranchName });
        //        }

        //        mainBranch.BranchID = branch.BranchID.Value;
        //        mainBranch.BranchName = branch.BranchName;
        //        result.CompanyArray.Add(mainBranch);
        //    }
        //    result.RequestmodeArray = (await _dbContext.GetAllAsync<RequestMode>()).ToList();
        //    result.LocationArray = await _dbContext.GetEnumerableAsync<LocationModel>("Select * from Location Where IsDeleted=0",null);
        //    result.MeterialTypeArray = await _dbContext.GetIdValuePairAsync<RequestMeterialType>("MeterialTypeName");
        //    result.SlotBefore = Convert.ToInt32(await _dbContext.GetAsync<int>("Select SettingsValue From GeneralSettings Where SettingsKey=@SettingsKey", new { SettingsKey="SlotSelectionBefore" }));

        //    result.AddedCoPassengers = await _commonRepository.GetCopassengers(CurrentUserID);

        //    result.PackingTypes = await _dbContext.GetIdValuePairAsync<PackingType>("PackingTypeName");
        //    result.AddedDesignations = (await _dbContext.GetAllAsync<EmployeeDesignation>()).ToList().Select(s => s.DesignationName);
        //    result.VehicleTypes = await _dbContext.GetIdValuePairAsync<VehicleType>("VehicleTypeName");
        //    result.AddedCompanies = (await _dbContext.GetAllAsyncByFieldName<Company>("AddedBy", CurrentUserID.ToString())).ToList().Select(s => s.CompanyName);
        //    result.AddedvehicleArray = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"Select VehicleID as ID, RegisterNo as Value 
        //        From viVehicle Where AddedBy=@CurrentUserID", new { CurrentUserID })).ToList();


        //    result.History = (await _dbContext.GetEnumerableAsync<RequestListViewModel>($@"SELECT RequestID, RequestNo, Convert(varchar, RequestedOn,103) Date, EmployeeName, BranchName, 
        //    SubBranchName, RequestedSlot, ModeName, 
        //    RequestedLocationName, Slot, LocationName, StatusID, T.RequestTypeName,Remarks
        //    FROM  viRequest R
        //    LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
        //    Where RequestedByID=CurrentUserID", new { CurrentUserID })).ToList();

        //    var employeeId = await _dbContext.GetAsync<int>($@"Select Top(1) EmployeeID From Employee Where AddedBy=@AddedBy and IsDeleted=0", new { AddedBy =CurrentUserID});
        //    result.Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, employeeId);

        //    //var url = _configuration["MainDomain"];
        //    //var webClient = new WebClient();
        //    //foreach (var item in result.Documents)
        //    //{
        //    //    try
        //    //    {
        //    //        if (!string.IsNullOrEmpty(item.FileName))
        //    //        {
        //    //            byte[] imageArray = webClient.DownloadData(url + item.FileName);
        //    //            item.Base64Image = "data:image/png;base64," + Convert.ToBase64String(imageArray);
        //    //        }
        //    //    }
        //    //    catch(Exception err) { }
        //    //}

        //    return result;
        //}

        //[HttpPost("get-slots")]
        //public async Task<SlotsViewModel> GetSlots(GetSlotPostModel Data)
        //{
        //    SlotsViewModel res = new SlotsViewModel()
        //    {
        //        Slots = await _dbContext.GetEnumerableAsync<IdnValuePair>($@"Select SlotID as ID,SlotName as Value
        //            From viSlot
        //            Where Date = @Date and BranchID = @BranchID and AvailableCount>0", Data)
        //    };
        //    return res;
        //}

        //[HttpPost("insert-meterial")]
        //public async Task<APIBaseResponse> InsertMeterial(MeterialRequestPostModel Data)
        //{
        //    APIBaseResponse result = new APIBaseResponse();

        //    string errorMsg = "";

        //    if (Data.CompanyId == 0)
        //        errorMsg = "Select Company!!";
        //    else if (Data.SubCompanyId == 0)
        //        errorMsg = "Select Sub Company!!";
        //    else if (Data.RequestMode == 0)
        //        errorMsg = "Select Request Mode!!";
        //    else if (Data.VisitDate == null)
        //        errorMsg = "Select Visit Date!!";
        //    else if (Data.TimeSlot == 0)
        //        errorMsg = "Select Time Slot!!";
        //    else if (Data.Location == 0)
        //        errorMsg = "Select Location!!";
        //    else if (Data.DriverId == 0)
        //        errorMsg = "Select Driver!!";
        //    else if (Data.VehicleId == 0)
        //        errorMsg = "Select Vehicle!!";
        //    else if (Data.Meterials.Count == 0 && Data.RequestMode != 2)
        //        errorMsg = "There is no material added!!";
        //    else if (!string.IsNullOrEmpty(Data.HostEmail))
        //    {
        //        var employeeId = await _dbContext.GetAsync<int?>($@"SELECT  Top(1) UserID
        //        FROM  Users
        //        Where ISNULL(IsDeleted,0)=0 and UserTypeID not in ({(int)UserTypes.Company},{(int)UserTypes.Individual}) 
        //        and (EmailAddress=@EmailAddress)", new { EmailAddress = Data.HostEmail });

        //        if (employeeId == null)
        //        {
        //            errorMsg = "Host not found!!";
        //        }
        //    }


        //    if (errorMsg != "")
        //    {
        //        result.CreateFailureResponse(errorMsg);
        //        return result;
        //    }


        //    cn.Open();
        //    using (var tran = cn.BeginTransaction())
        //    {
        //        try
        //        {
        //            var employeeId = await _dbContext.GetAsync<int>($@"Select Top(1) EmployeeID From Employee Where AddedBy={CurrentUserID} and IsDeleted=0", null, tran);

        //            Request request;
        //            if (Data.RequestID == 0)
        //            {
        //                request = new();
        //                request.RequestNo = await _dbContext.GetAsync<int>("Select ISNULL(Max(RequestNo),0)+1 from Request", null, tran);
        //            }
        //            else
        //            {
        //                request = await _dbContext.GetAsync<Request>(Data.RequestID,tran);
        //            }

        //            request.Date = Data.VisitDate;
        //            request.BranchID = Data.CompanyId;
        //            request.SubBranchID = Data.SubCompanyId;
        //            request.ContainsExplosive = Data.ContainsExplosive;
        //            request.EmployeeID = employeeId;
        //            request.HostEmail = Data.HostEmail;
        //            request.IsDisposalRequired = Data.IsDisposalRequired;
        //            request.LocationID = Data.Location;
        //            request.MeterialTypeID = Data.MeterialType;
        //            request.Narration = Data.Narration;
        //            request.RequestModeID = Data.RequestMode;
        //            request.SlotID = Data.TimeSlot;
        //            request.FromApp = true;
        //            var requestId = await _dbContext.SaveAsync(request, tran);

        //            if (Data.Meterials.Count > 0)
        //            {
        //                var meterials = new List<RequestMeterial>();
        //                foreach (var item in Data.Meterials)
        //                {
        //                    meterials.Add(new RequestMeterial() { RequestMeterialID=item.RequestMeterialID==0?null: item.RequestMeterialID, Description = item.Description, PackingTypeID = item.PackingTypeID, Quantity = item.Quantity });
        //                }
        //                await _dbContext.SaveSubListAsync(meterials, "RequestID", requestId, tran);
        //            }

        //            var passengers = new List<RequestPassenger>();
        //            if (Data.CoPassangerId.Count > 0)
        //            {
        //                await _dbContext.DeleteSubItemsAsync<RequestPassenger>("RequestID", requestId, tran);
        //                foreach (var item in Data.CoPassangerId)
        //                {
        //                    passengers.Add(new RequestPassenger() { EmployeeID = item });
        //                }

        //                await _dbContext.SaveSubListAsync(passengers, "RequestID", requestId, tran);
        //            }

        //            await _dbContext.DeleteSubItemsAsync<RequestVehicle>("RequestID", Data.RequestID, tran);
        //            RequestVehicle Vehicles = new()
        //            {
        //                RequestID = requestId,
        //                VehicleID = Data.VehicleId,
        //                EmployeeID = Data.DriverId,
        //                PassengerCount = passengers.Count()
        //            };
        //            await _dbContext.SaveAsync(Vehicles, tran);

        //            if (Data.Documents != null)
        //            {
        //                foreach (var document in Data.Documents)
        //                {
        //                    var doc = await _dbContext.GetAsync<Document>($"Select * from Document where DocumentTypeID={document.DocumentType} and EmployeeID={employeeId}", null, tran);
        //                    if (doc == null)
        //                    {
        //                        doc = new Document();
        //                    }
        //                    doc.DocumentNumber = document.DocumentNo;
        //                    doc.DocumentTypeID = document.DocumentType;
        //                    doc.ExpiresOn = document.DocumentExpiry;
        //                    doc.EmployeeID = employeeId;
        //                    if (document.MediaID != 0)
        //                        doc.MediaID = document.MediaID;
        //                    if (document.MediaID2 != 0)
        //                        doc.MediaID2 = document.MediaID2;
        //                    await _dbContext.SaveAsync(doc, tran);
        //                }
        //            }

        //            await _dbContext.DeleteSubItemsAsync<RequestApproval>("RequestID", requestId, tran);

        //            #region Mail to appprovers

        //            var url = _configuration["MainDomain"];
        //            await _commonRepository.SendNewRequestMailToApprover(requestId, url, tran);

        //            #endregion

        //            tran.Commit();
        //            result.Message = "Your request has been registered successfully";
        //        }
        //        catch (Exception err)
        //        {
        //            tran.Rollback();
        //            result.CreateFailureResponse(err.Message);
        //        }

        //        return result;
        //    }
        //}

        //[HttpPost("get-request-details")]
        //public async Task<MeterialRequestViewModel> GetRequestDetails(RequestIDModel model)
        //{
        //    MeterialRequestViewModel result = new();
        //    result = await _dbContext.GetAsync<MeterialRequestViewModel>($@"Select R.RequestID,RequestNo,HostEmail,BranchID CompanyID,
        //        SubBranchID SubCompanyID,ContainsExplosive,LocationID Location,IsDisposalRequired,
        //        MeterialTypeID MeterialType,Narration,RequestModeID as RequestMode,
        //        VehicleID,PassengerCount,V.EmployeeID as DriverID
        //        From Request R
        //        JOIN RequestVehicle V on V.RequestID=R.RequestID and V.IsDeleted=0
        //        Where R.RequestID=@RequestID", new { RequestID = model.RequestID });

        //    result.CoPassangerId = (await _dbContext.GetEnumerableAsync<int>($@"Select EmployeeID
        //        From RequestPassenger
        //        Where RequestID=@RequestID", new { RequestID = model.RequestID })).ToList();

        //    result.Meterials = (await _dbContext.GetEnumerableAsync<RequestMeterialPostModel>($@"Select RequestMeterialID,Description,Quantity,PackingTypeID
        //        From RequestMeterial 
        //        Where RequestID=@RequestID and IsDeleted=0", new { RequestID = model.RequestID })).ToList();

        //    result.Documents = await _commonRepository.GetAllDocumentsAsync(model.RequestID);
            
        //    return result;

        //}
    }
}
