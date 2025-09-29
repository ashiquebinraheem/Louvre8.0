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

namespace Progbiz.API.Controllers
{
    [Route("api/visitor")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles = "Visitor")]
    public class VisitorController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public VisitorController(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

     //   [HttpGet("get-data")]
     //   public async Task<HomeDetail> GetHomeData()
     //   {
     //       HomeDetail result = new HomeDetail();

     //       var clientDate = CurrentClientTime;

     //       var visitrequestCount = await _dbContext.ExecuteScalarAsync<int>($@"Select COUNT(VisitRequestID) From viVisitRequest WHERE (RequestedByID ={CurrentUserID}) and MeetingDate>=@Date",new { Date= clientDate.Date});
     //       result.TotalVisitCount = visitrequestCount;
     //       var summary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT StatusID as ID,Count(StatusID) as Value FROM viVisitRequest WHERE (RequestedByID = {CurrentUserID}) and MeetingDate>=@Date' Group by StatusID", new { Date= clientDate.Date })).ToList();

     //       result.PendingCount = Convert.ToInt32(summary.Where(s => s.ID == (int)RequestStatus.Pending).Select(s => s.Value).FirstOrDefault() ?? "0");
     //       result.ApprovedVisitCount = Convert.ToInt32(summary.Where(s => s.ID == (int)RequestStatus.Accepted).Select(s => s.Value).FirstOrDefault() ?? "0");
     //       result.RejectedCount = Convert.ToInt32(summary.Where(s => s.ID == (int)RequestStatus.Rejected).Select(s => s.Value).FirstOrDefault() ?? "0");

     //       result.DepartmentArray = (await _dbContext.GetIdValuePairAsync<Department>("DepartmentName")).ToList();
     //       result.AreaArray = (await _dbContext.GetIdValuePairAsync<Area>("AreaName")).ToList();
     //       result.PurposeArray = (await _dbContext.GetIdValuePairAsync<Purpose>("PurposeName")).ToList();
     //       result.DurationArray = (await _dbContext.GetIdValuePairAsync<Duration>("DurationName")).ToList();
     //       result.AreaArray = (await _dbContext.GetIdValuePairAsync<Area>("AreaName")).ToList();
     //       result.AddedvehicleArray = (await _dbContext.GetEnumerableAsync<ViVehicle>($@"Select VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo, VehicleMakeName, 
     //           VehiclePlateSourceName, VehiclePlateTypeName, VehiclePlateCategoryName 
     //           From viVehicle Where AddedBy=@CurrentUserID", new { CurrentUserID })).ToList();
     //       result.VisiterProfileArray = await _commonRepository.GetEmployeesAsync(CurrentUserID);
     //       result.SlotBefore = Convert.ToInt32(await _dbContext.GetAsync<int>("Select SettingsValue From GeneralSettings Where SettingsKey=@SettingsKey", new { SettingsKey = "VisitSelectionBefore" }));

     //       result.History = (await _dbContext.GetEnumerableAsync<VisitRequestListViewModel>($@"Select VisitRequestID, EmployeeName, DepartmentName, AreaName, PurposeName, convert(varchar, MeetingDate, 100) MeetingDate, DurationName, ISNULL(Remark,'') as Remark, StatusID
     //       From viVisitRequest
     //       Where RequestedByID=@CurrentUserID",new { CurrentUserID})).ToList();

     //       var employeeId = await _dbContext.GetAsync<int>($@"Select Top(1) EmployeeID From Employee Where AddedBy=@AddedBy and IsDeleted=0", new { AddedBy =CurrentUserID});
     //       result.Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, employeeId);
     //       var personalInfo = await _commonRepository.GetProfileInfo(CurrentUserID);
     //       result.QRCode = _mediaRepository.GetQRImage(personalInfo.QRCode);
     //       result.Name = User.Identity.Name;

     //       return result;
     //   }

     //   [HttpPost("insert-visit")]
     //   public async Task<APIBaseResponse> InsertVisit(VisitRequestPostModel model)
     //   {
     //       APIBaseResponse result = new APIBaseResponse();

     //       var hostEmployee = await _dbContext.GetAsync<User>($@"SELECT  Top(1) *
     //       FROM  Users
     //       Where ISNULL(IsDeleted,0)=0 and UserTypeID not in ({(int)UserTypes.Company},{(int)UserTypes.Individual}) 
     //       and (EmailAddress=@HostDetail or MobileNumber=@HostDetail)", new { HostDetail = model.HostDetail });

     //       var employeeId = await _dbContext.GetAsync<int>($@"Select Top(1) EmployeeID From Employee Where AddedBy=@AddedBy 
     //           and IsDeleted=0", new { AddedBy = CurrentUserID });

     //       if (hostEmployee == null)
     //       {
     //           result.CreateFailureResponse("Host not found");
     //           return result;
     //       }

     //       VisitRequest Data = new VisitRequest()
     //       {
     //           VisitRequestID=model.VisitRequestID,
     //           AreaID=model.AreaId,
     //           DuraionID=model.Duration,
     //           DepartmentID=model.DeartmentId,
     //           EmployeeID=employeeId,
     //           HostUserID= hostEmployee.UserID,
     //           MeetingDate=model.MeetingDate,
     //           PurposeID=model.Purpose,
     //           Remark=model.Remarks,
     //           VehicleID= model.VehicleId
     //       };

     //       cn.Open();
     //       using var tran = cn.BeginTransaction();
     //       try
     //       {

     //           var requestId = await _dbContext.SaveAsync(Data, tran);

     //           if (model.Documents != null)
     //           {
     //               foreach (var document in model.Documents)
     //               {
     //                   var doc = await _dbContext.GetAsync<Document>($"Select * from Document where DocumentTypeID={document.DocumentType} and EmployeeID={employeeId}", null, tran);
     //                   if (doc == null)
     //                   {
     //                       doc = new Document();
     //                   }
     //                   doc.DocumentNumber = document.DocumentNo;
     //                   doc.DocumentTypeID = document.DocumentType;
     //                   doc.ExpiresOn = document.DocumentExpiry;
     //                   doc.EmployeeID = employeeId;
     //                   if (document.MediaID != 0)
     //                       doc.MediaID = document.MediaID;
     //                   if (document.MediaID2 != 0)
     //                       doc.MediaID2 = document.MediaID2;
     //                   await _dbContext.SaveAsync(doc, tran);
     //               }
     //           }


     //           #region Mail

     //           //var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
     //           var url = _configuration["MainDomain"];

     //           var requestData = await _dbContext.GetAsync<VisitRequestView>($@"Select * 
     //               From viVisitRequest 
     //               Where VisitRequestID={requestId}", null, tran);

     //           var body = $@"<!DOCTYPE html>
					//<html>
					//<head>
					//<title>New Visit Request From {requestData.Requester}</title>
					//<link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
					//</head>
					//<body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>

					//    <div style='width:600px;height:80vh;background-color:#fff;margin: 0 auto;margin-top:50px;-webkit-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-moz-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-o-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);box-shadow: 0px 0px 20px rgba(0,0,0,0.1);border-radius: 10px;padding: 30px;'>
					
     //                       <div style='background-color:none;width:50%;float: left;'>
					//	        <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Meeting Date:</span> {requestData.MeetingDate}</p>
					//        </div>
    
					//        <div style='background-color:none;width:50%;float: left;'>
					//	        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Duration:</span> {requestData.DurationName}</p>
					//        </div>

					//        <div style='background-color:none;width:100%;float: left;'>
					//	        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Pupose:</span> {requestData.PurposeName}</p>
					//        </div>
    
					//         <div style='background-color:none;width:100%;float: left;'>
					//	        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Remarks:</span> {requestData.Remark}</p>
					//        </div>

					//        <div style='background-color:none;width:100%;float: left;'>
					//	        <p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'></span><a href='{url}/approve-visit-request-mail/{requestData.VisitRequestID}/{requestData.QRCode}'>click here to approve the request</a></p>
					//        </div>
     //                   </div>
     //               </body>    
					//</html>";
     //           await _emailSender.SendEmailAsync(hostEmployee.EmailAddress, "New Visit Request", body, tran);


     //           #endregion


     //           tran.Commit();
     //           result.Message="Your request has been submitted for verification";
     //       }
     //       catch (Exception err)
     //       {
     //           tran.Rollback();
     //           result.CreateFailureResponse(err.Message);
     //       }

     //       return result;
     //   }


     //   [HttpPost("get-visit-details")]
     //   public async Task<VisitRequestViewModel> GetVisitDetails(VisitRequestIDModel model)
     //   {
     //       var result = await _dbContext.GetAsync<VisitRequestViewModel>($@"Select VisitRequestID,AreaID,R.DuraionID as Duration,DepartmentID,EmployeeID,U.EmailAddress as HostDetail,MeetingDate,PurposeID as Purpose,Remark as Remarks,VehicleID
     //           from VisitRequest R
     //           LEFT JOIN Users U on U.UserID=R.HostUserID
     //           Where R.VisitRequestID=@VisitRequestID", new { VisitRequestID = model.VisitRequestID });
     //       result.Documents = await _commonRepository.GetAllVisitRequestDocumentsAsync(model.VisitRequestID);
     //       return result;
     //   }
    }
}
 