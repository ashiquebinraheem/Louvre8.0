using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Meterial")]
    [BindProperties]
    public class ExitRequestOldModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IErrorLogRepository _errorLogRepo;

        public ExitRequestOldModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IHttpContextAccessor httpContextAccessor, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _httpContextAccessor = httpContextAccessor;
            _errorLogRepo = errorLogRepo;
        }

        public Request Data { get; set; }
        public RequesterPostViewModel Employee { get; set; }
        public Company Company { get; set; }
        public List<RequestMeterial> Meterials { get; set; }

        //public List<RequestVehicle> Vehicles { get; set; }
        public RequestVehicle Vehicles { get; set; }

        public List<RequestPassenger> Passengers { get; set; }

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
            ViewData["EmployeesSelectList"] = new SelectList(employees.Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.EmployeeID), Value = s.EmployeeName }), "ID", "Value");
            ViewData["Companies"] = (await _dbContext.GetAllAsyncByFieldName<Company>("AddedBy", CurrentUserID.ToString())).ToList().Select(s => s.CompanyName);
            ViewData["Designations"] = (await _dbContext.GetAllAsync<EmployeeDesignation>()).ToList().Select(s => s.DesignationName);
            ViewData["PackingTypes"] = new SelectList((await _dbContext.GetAllAsync<PackingType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.PackingTypeID), Value = s.PackingTypeName }), "ID", "Value");
            ViewData["VehicleTypes"] = new SelectList((await _dbContext.GetAllAsync<VehicleType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.VehicleTypeID), Value = s.VehicleTypeName }), "ID", "Value");
            ViewData["Vehicles"] = new SelectList((await _dbContext.GetAllAsyncByFieldName<Vehicle>("AddedBy", CurrentUserID.ToString())).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.VehicleID), Value = s.RegisterNo }), "ID", "Value");
            ViewData["Branches"] = await _dbContext.GetAllAsync<Branch>();
            ViewData["RequestModes"] = (await _dbContext.GetAllAsync<RequestMode>()).ToList().Where(s => s.IsIn == false);
            ViewData["Locations"] = await _dbContext.GetAllAsync<Location>();
            ViewData["MaterialTypes"] = new SelectList((await _dbContext.GetAllAsync<RequestMeterialType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.MeterialTypeID), Value = s.MeterialTypeName }), "ID", "Value");
            if (id != null)
            {
                Data = await _dbContext.GetAsync<Request>(Convert.ToInt32(id));
                if (Data != null && Data.AddedBy != CurrentUserID)
                {
                    Data = null;
                }
            }

            if (Data != null)
            {
                Meterials = (await _dbContext.GetAllAsyncByFieldName<RequestMeterial>("RequestID", id.ToString())).ToList();
                Vehicles = await _dbContext.GetAsyncByFieldName<RequestVehicle>("RequestID", id.ToString());
                Passengers = (await _dbContext.GetAllAsyncByFieldName<RequestPassenger>("RequestID", id.ToString())).ToList();
                var employee = await _dbContext.GetAsync<Employee>(Convert.ToInt32(Data.EmployeeID));
                //CompanyDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, Convert.ToInt32(employee.CompanyID));
                RequestDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Request, Convert.ToInt32(Data.RequestID));
                RequestStatusID = await _dbContext.ExecuteScalarAsync<int>($"Select StatusID From viRequest where RequestID={Data.RequestID}", null);
            }
            else
            {
                RequestDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Request, 0);
            }
            CompanyDocuments = new List<DocumentPostViewModel>();
            EmployeeDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, 0);
            VehicleDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Vehicle, 0);
            Meterials = Meterials ?? new List<RequestMeterial>();
            Passengers = Passengers ?? new List<RequestPassenger>();

            var slotBefore = Convert.ToInt32(await _dbContext.GetAsync<int>("Select SettingsValue From GeneralSettings Where SettingsKey=@SettingsKey", new { SettingsKey= "SlotSelectionBefore" }));
            var clientDate = GetClientTime(_httpContextAccessor);
            ViewData["FromDate"] = clientDate.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = clientDate.AddDays(slotBefore).ToString("yyyy-MM-dd");
            ViewData["Today"] = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {

            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    if (Data.RequestNo == 0)
                    {
                        Data.RequestNo = await _dbContext.GetAsync<int>("Select ISNULL(Max(RequestNo),0)+1 from Request", null, tran);
                    }

                    var requestId = await _dbContext.SaveAsync(Data, tran);

                    await _dbContext.SaveSubListAsync(Meterials, "RequestID", requestId, tran);

                    await _dbContext.SaveSubListAsync(Passengers, "RequestID", requestId, tran);

                    Vehicles.RequestID = requestId;
                    Vehicles.PassengerCount = Passengers.Count();
                    await _dbContext.SaveAsync(Vehicles, tran);

                    #region Company Documents

                    //var employee = await _dbContext.GetAsync<Employee>(Convert.ToInt32(Data.EmployeeID), tran);

                    var documents = new List<Document>();

                    for (int i = 0, j = 0; i < CompanyDocuments.Count; i++)
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
                    await _dbContext.SaveSubListAsync(documents, "CompanyID", Convert.ToInt32(CompanyID), tran);

                    #endregion

                    #region Request Documents


                    documents = new List<Document>();

                    for (int i = 0, j = 0; i < RequestDocuments.Count; i++)
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
                            j++;
                        }

                        documents.Add(document);
                    }
                    await _dbContext.SaveSubListAsync(documents, "RequestID", requestId, tran);

                    #endregion

                    #region Mail to appprovers

                    var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    await _commonRepository.SendNewRequestMailToApprover(requestId, url, tran);

                    #endregion

                    tran.Commit();
                    result.CreatSuccessResponse(101);
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

                    for (int i = 0, j = 0; i < VehicleDocuments.Count; i++)
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
                        From viVehicle Where AddedBy=@CurrentUserID", new { CurrentUserID })).ToList();
                    result.NewVehicleID = vehicleId;
                    result.CreatSuccessResponse(1);
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

        public async Task<IActionResult> OnPostLoadSlotAsync()
        {
            var slots = await _dbContext.GetEnumerableAsync<IdnValuePair>($@"Select SlotID as ID,SlotName as Value
            From viSlot
            Where Date = @Date and BranchID = @BranchID and AvailableCount>0", Data);

            return new JsonResult(slots);
        }
    }
}