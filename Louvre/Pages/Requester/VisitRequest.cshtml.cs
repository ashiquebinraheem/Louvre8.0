using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Roles = "Visitor")]
    [BindProperties]
    public class VisitRequestModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IErrorLogRepository _errorLogRepo;

        public VisitRequestModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _errorLogRepo = errorLogRepo;
        }

        public VisitRequest Data { get; set; }
        [Required(ErrorMessage = "Enter Host Details")]
        public string? HostDetail { get; set; }

        public RequesterPostViewModel Employee { get; set; }
        public Company Company { get; set; }
        public List<VisitRequestDrink> Drinks { get; set; }

        public List<IFormFile>? EmpDocuments { get; set; }
        public List<DocumentPostViewModel> EmployeeDocuments { get; set; }

        public Vehicle Vehicle { get; set; }
        public List<IFormFile>? VehDocuments { get; set; }
        public List<DocumentPostViewModel> VehicleDocuments { get; set; }


        public List<IFormFile>? CompDocuments { get; set; }
        public List<DocumentPostViewModel> CompanyDocuments { get; set; }

        public List<IFormFile>? ReqDocuments { get; set; }
        public List<DocumentPostViewModel> RequestDocuments { get; set; }

        public int CompanyID { get; set; }

        public int RequestStatusID { get; set; }

        public async Task OnGetAsync(int? id)
        {
            var user = await _dbContext.GetAsync<User>(CurrentUserID);
            if (!user.IsApproved)
            {
                ViewData["NotApproved"] = "1";
            }

            var employees = await _commonRepository.GetEmployeesAsync(CurrentUserID);
            ViewData["Employees"] = employees;
            ViewData["Vehicles"] = (await _dbContext.GetEnumerableAsync<ViVehicle>($@"Select VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo, VehicleMakeName, 
                VehiclePlateSourceName, VehiclePlateTypeName, VehiclePlateCategoryName 
                From viVehicle Where AddedBy=@CurrentUserID", new { CurrentUserID })).ToList();
            //ViewData["Vehicles"] = await GetSelectList<Vehicle>(_dbContext, "RegisterNo", $"AddedBy ={ CurrentUserID }");
            ViewData["Companies"] = (await _dbContext.GetAllAsyncByFieldName<Company>("AddedBy", CurrentUserID.ToString())).ToList().Select(s => s.CompanyName);
            ViewData["Designations"] = (await _dbContext.GetAllSelectedFieldAsync<EmployeeDesignation, string>("DesignationName")).ToList();
            ViewData["Countries"] = await GetSelectList<Country>(_dbContext, "CountryName");
            ViewData["Departments"] = await GetSelectList<Department>(_dbContext, "DepartmentName");
            ViewData["Areas"] = await GetSelectList<Area>(_dbContext, "AreaName");
            ViewData["Purposes"] = await GetSelectList<Purpose>(_dbContext, "PurposeName");
            ViewData["Durations"] = await GetSelectList<Duration>(_dbContext, "DurationName");
            ViewData["VehicleTypes"] = await GetSelectList<VehicleType>(_dbContext, "VehicleTypeName");
            ViewData["VehicleMakes"] = await GetSelectList<VehicleMake>(_dbContext, "VehicleMakeName");
            ViewData["VehiclePlateSources"] = await GetSelectList<VehiclePlateSource>(_dbContext, "VehiclePlateSourceName");
            ViewData["VehiclePlateCategories"] = await GetSelectList<VehiclePlateCategory>(_dbContext, "VehiclePlateCategoryName");
            ViewData["VehiclePlateTypes"] = await GetSelectList<VehiclePlateType>(_dbContext, "VehiclePlateTypeName");

            if (id != null)
            {
                Data = await _dbContext.GetAsync<VisitRequest>(Convert.ToInt32(id));
                if (Data != null && Data.AddedBy != CurrentUserID)
                {
                    Data = null;
                }
            }

            if (Data != null)
            {
                var employee = await _dbContext.GetAsync<Employee>(Convert.ToInt32(Data.EmployeeID));
                CompanyDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, Convert.ToInt32(employee.CompanyID));
                RequestDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.VisitRequest, Convert.ToInt32(Data.VisitRequestID));
                RequestStatusID = await _dbContext.ExecuteScalarAsync<int>($"Select StatusID From viVisitRequest where VisitRequestID={Data.VisitRequestID}", null);

                if (Data.HostUserID.HasValue)
                    HostDetail = (await _dbContext.GetAsync<User>(Data.HostUserID.Value)).EmailAddress;
            }
            else
            {
                RequestDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.VisitRequest, 0);
                CompanyDocuments = new List<DocumentPostViewModel>();
            }
            EmployeeDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, 0);
            VehicleDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Vehicle, 0);

            var slotBefore = Convert.ToInt32(await _dbContext.GetAsync<int>("Select SettingsValue From GeneralSettings Where SettingsKey=@SettingsKey", new { SettingsKey= "VisitSelectionBefore" }));
            var clientDate = GetClientTime(_httpContextAccessor);
            ViewData["FromDate"] = clientDate.Date.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = clientDate.AddDays(slotBefore).Date.ToString("yyyy-MM-dd");
            ViewData["Today"] = DateTime.Today.ToString("yyyy-MM-dd");
        }




        public async Task<IActionResult> OnPostSaveAsync()
        {
            BaseResponse result = new BaseResponse();

            #region Rate Limiting

            var rateLimitCnt = await _dbContext.GetAsync<int>($@"Select Count(*) From VisitRequest 
            Where ISNULL(IsDeleted,0)=0 and AddedBy={CurrentUserID} and AddedOn>DATEADD(MINUTE,-5,GETUTCDATE())", null);

            if (rateLimitCnt > 0)
            {
                result.CreatErrorResponse("You’ve already submitted a request recently. Please wait at least 5 minutes before submitting another one.", "Duplicate Request");
                return new JsonResult(result);
            }

            #endregion


            var employeeId = await _dbContext.GetAsync<int?>($@"SELECT  Top(1) UserID
            FROM  Users
            Where ISNULL(IsDeleted,0)=0 and UserTypeID not in ({(int)UserTypes.Company},{(int)UserTypes.Individual}) and (EmailAddress=@HostDetail or MobileNumber=@HostDetail)", new { HostDetail = HostDetail });

            if (employeeId == null)
            {
                result.CreatErrorResponse(-107);
                return new JsonResult(result);
            }

            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    Data.HostUserID = employeeId;

                    var requestId = await _dbContext.SaveAsync(Data, tran);

                    await _dbContext.SaveSubListAsync(Drinks, "VisitRequestID", requestId, tran);

                    #region Company Documents

                    var documents = new List<Document>();

                    for (int i = 0, j = 0; i < CompanyDocuments.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(CompanyDocuments[i].DocumentNumber))
                        {
                            var document = new Document
                            {
                                DocumentID = CompanyDocuments[i].DocumentID,
                                DocumentTypeID = CompanyDocuments[i].DocumentTypeID,
                                ExpiresOn = CompanyDocuments[i].ExpiresOn,
                                DocumentNumber = CompanyDocuments[i].DocumentNumber,
                                MediaID = CompanyDocuments[i].MediaID
                            };

                            if (CompanyDocuments[i].HasFile)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(CompanyDocuments[i].MediaID, CompDocuments[j], "company_documents", CompanyID + "_" + CompanyDocuments[i].DocumentTypeID, tran);
                                if (mediaResult.IsSuccess)
                                {
                                    document.MediaID = mediaResult.MediaID;
                                }
                                else
                                    document.MediaID = null;
                                j++;
                            }

                            documents.Add(document);
                        }
                    }
                    await _dbContext.SaveSubListAsync(documents, "CompanyID", Convert.ToInt32(CompanyID), tran);

                    #endregion

                    #region Request Documents

                    documents = new List<Document>();

                    for (int i = 0, j = 0; i < RequestDocuments.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(RequestDocuments[i].DocumentNumber))
                        {
                            var document = new Document
                            {
                                DocumentID = RequestDocuments[i].DocumentID,
                                DocumentTypeID = RequestDocuments[i].DocumentTypeID,
                                ExpiresOn = RequestDocuments[i].ExpiresOn,
                                DocumentNumber = RequestDocuments[i].DocumentNumber,
                                MediaID = RequestDocuments[i].MediaID
                            };

                            if (RequestDocuments[i].HasFile)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(RequestDocuments[i].MediaID, ReqDocuments[j], "request_documents", requestId + "_" + RequestDocuments[i].DocumentTypeID, tran);
                                if (mediaResult.IsSuccess)
                                {
                                    document.MediaID = mediaResult.MediaID;
                                }
                                else
                                    document.MediaID = null;
                                j++;
                            }


                            documents.Add(document);
                        }
                    }
                    await _dbContext.SaveSubListAsync(documents, "VisitRequestID", requestId, tran);

                    #endregion


                    #region Mail

                    var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

                    var requestData = await _dbContext.GetAsync<VisitRequestView>($@"Select * 
                    From viVisitRequest 
                    Where VisitRequestID={requestId}", null, tran);

                    var body = $@"<!DOCTYPE html>
					<html>
					<head>
					<title>New Visit Request From {requestData.Requester}</title>
					<link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
					</head>
					<body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>

					    <div style='width:600px;height:80vh;background-color:#fff;margin: 0 auto;margin-top:50px;-webkit-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-moz-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-o-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);box-shadow: 0px 0px 20px rgba(0,0,0,0.1);border-radius: 10px;padding: 30px;'>
					
                            <div style='background-color:none;width:50%;float: left;'>
						        <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Meeting Date:</span> {requestData.MeetingDate}</p>
					        </div>
    
					        <div style='background-color:none;width:50%;float: left;'>
						        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Duration:</span> {requestData.DurationName}</p>
					        </div>

					        <div style='background-color:none;width:100%;float: left;'>
						        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Pupose:</span> {requestData.PurposeName}</p>
					        </div>
    
					         <div style='background-color:none;width:100%;float: left;'>
						        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Remarks:</span> {requestData.Remark}</p>
					        </div>

					        <div style='background-color:none;width:100%;float: left;'>
						        <p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'></span><a href='{url}/approve-visit-request-mail/{requestData.VisitRequestID}/{requestData.QRCode}'>click here to approve the request</a></p>
					        </div>
                        </div>
                    </body>    
					</html>";
                    await _emailSender.SendEmailAsync(requestData.EmployeeEmailAddress, "New Visit Request", body, tran);


                    #endregion


                    tran.Commit();
                    result.CreatSuccessResponse(101);
                }
                catch (PreDefinedException err)
                {
                    tran.Rollback();
                    throw err;
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                }

                return new JsonResult(result);
            }
        }

        public async Task<IActionResult> OnPostSaveEmployeeAsync()
        {
            #region Rate Limiting

            var rateLimitCnt = await _dbContext.GetAsync<int>($@"Select Count(*) From Employee 
            Where ISNULL(IsDeleted,0)=0 and AddedBy={CurrentUserID} and AddedOn>DATEADD(MINUTE,-1,GETUTCDATE())", null);

            if (rateLimitCnt > 0)
            {
                BaseResponse r = new BaseResponse();
                r.CreatErrorResponse("You’ve already added an employee recently. Please wait at least 1 minutes before submitting another one.", "Duplicate Request");
                return new JsonResult(r);
            }

            #endregion

            var isExist = await _dbContext.GetAsyncByFieldName<Employee>("EmployeeName", Employee.EmployeeName);
            if (isExist != null && isExist.EmployeeID != Data.EmployeeID && isExist.AddedBy == CurrentUserID)
            {
                var response = new BaseResponse(-104);
                return new JsonResult(response);
            }

            RequesterSaveResponseViewModel result = new RequesterSaveResponseViewModel();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {

                    bool isFresh = false;
                    string qrcode;
                    do
                    {
                        Random random = new Random();
                        qrcode = random.Next(10000000, 99999999).ToString();
                        var req = await _dbContext.GetAsyncByFieldName<Employee>("QRCode", qrcode, tran);
                        if (req == null)
                            isFresh = true;
                    } while (isFresh == false);



                    Employee employee = new Employee()
                    {
                        EmployeeName = Employee.EmployeeName,
                        CompanyID = await _commonRepository.GetCompanyID(Employee.CompanyName, CurrentUserID, tran),
                        DesignationID = await _commonRepository.GetDesignationID(Employee.DesignationName, tran),
                        ContactNumber = Employee.ContactNumber,
                        Email = Employee.Email,
                        QRCode = qrcode
                    };
                    var employeeId = await _dbContext.SaveAsync(employee, tran);


                    #region Documents

                    var documents = new List<Document>();

                    for (int i = 0, j = 0; i < EmployeeDocuments.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(EmployeeDocuments[i].DocumentNumber))
                        {
                            var document = new Document
                            {
                                DocumentID = EmployeeDocuments[i].DocumentID,
                                DocumentTypeID = EmployeeDocuments[i].DocumentTypeID,
                                ExpiresOn = EmployeeDocuments[i].ExpiresOn,
                                DocumentNumber = EmployeeDocuments[i].DocumentNumber,
                                MediaID = EmployeeDocuments[i].MediaID
                            };

                            if (EmployeeDocuments[i].HasFile)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(EmployeeDocuments[i].MediaID, EmpDocuments[j], "employee_documents", employeeId + "_" + EmployeeDocuments[i].DocumentTypeID, tran);
                                if (mediaResult.IsSuccess)
                                {
                                    document.MediaID = mediaResult.MediaID;
                                }
                                j++;
                            }

                            documents.Add(document);
                        }
                    }
                    await _dbContext.SaveSubListAsync(documents, "EmployeeID", employeeId, tran);

                    #endregion

                    tran.Commit();
                    result.Employees = await _commonRepository.GetEmployeesAsync(CurrentUserID, tran);
                    result.NewEmployeeID = employeeId;
                    result.CreatSuccessResponse(1);
                }
                catch (PreDefinedException err)
                {
                    tran.Rollback();
                    throw err;
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    var s = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                    result.CreatThrowResponse(s.ResponseErrorDescription);
                }

                return new JsonResult(result);
            }
        }

        public async Task<IActionResult> OnPostSaveVehicleAsync()
        {
            #region Rate Limiting

            var rateLimitCnt = await _dbContext.GetAsync<int>($@"Select Count(*) From Vehicle 
            Where ISNULL(IsDeleted,0)=0 and AddedBy={CurrentUserID} and AddedOn>DATEADD(MINUTE,-1,GETUTCDATE())", null);

            if (rateLimitCnt > 0)
            {
                BaseResponse r = new BaseResponse();
                r.CreatErrorResponse("You’ve already added an vehicle recently. Please wait at least 1 minutes before submitting another one.", "Duplicate Request");
                return new JsonResult(r);
            }

            #endregion

            var isExist = await _dbContext.GetAsyncByFieldName<Vehicle>("RegisterNo", Vehicle.RegisterNo);
            if (isExist != null && isExist.VehicleID != Vehicle.VehicleID && isExist.AddedBy == CurrentUserID)
            {
                var response = new BaseResponse(-105);
                return new JsonResult(response);
            }

            VehicleSaveResponseViewModel result = new VehicleSaveResponseViewModel();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    var vehicleId = await _dbContext.SaveAsync(Vehicle, tran);


                    #region Documents

                    var documents = new List<Document>();

                    for (int i = 0, j = 0; i < EmployeeDocuments.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(VehicleDocuments[i].DocumentNumber))
                        {
                            var document = new Document
                            {
                                DocumentID = VehicleDocuments[i].DocumentID,
                                DocumentTypeID = VehicleDocuments[i].DocumentTypeID,
                                ExpiresOn = VehicleDocuments[i].ExpiresOn,
                                DocumentNumber = VehicleDocuments[i].DocumentNumber,
                                MediaID = VehicleDocuments[i].MediaID
                            };

                            if (VehicleDocuments[i].HasFile)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(VehicleDocuments[i].MediaID, VehDocuments[j], "vehicle_documents", vehicleId + "_" + VehicleDocuments[i].DocumentTypeID, tran);
                                if (mediaResult.IsSuccess)
                                {
                                    document.MediaID = mediaResult.MediaID;
                                }
                                j++;
                            }

                            documents.Add(document);
                        }
                    }
                    await _dbContext.SaveSubListAsync(documents, "VehicleID", vehicleId, tran);

                    #endregion

                    tran.Commit();
                    result.Vehicles = (await _dbContext.GetEnumerableAsync<ViVehicle>($@"Select VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo, VehicleMakeName, 
                        VehiclePlateSourceName, VehiclePlateTypeName, VehiclePlateCategoryName 
                        From viVehicle Where AddedBy=@CurrentUserID", new
                    {
                        CurrentUserID
                    })).ToList();
                    result.NewVehicleID = vehicleId;
                    result.CreatSuccessResponse(1);
                }
                catch (PreDefinedException err)
                {
                    tran.Rollback();
                    throw err;
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    var s = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                    result.CreatThrowResponse(s.ResponseErrorDescription);
                }

                return new JsonResult(result);
            }
        }

        public async Task<IActionResult> OnPostLoadCompanyDocumentsAsync()
        {
            CompanyDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, CompanyID);
            return new JsonResult(CompanyDocuments);
        }
    }
}