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
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize]
    [BindProperties]
    public class ViewRequestModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly ICommonRepository _commonRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediaRepository _mediaRepository;

        public ViewRequestModel(IDbContext dbContext, ICommonRepository commonRepository, IHttpContextAccessor httpContextAccessor, IMediaRepository mediaRepository)
        {
            _dbContext = dbContext;
            _commonRepository = commonRepository;
            _httpContextAccessor = httpContextAccessor;
            _mediaRepository = mediaRepository;
        }

        public RequestApprovalHeaderView Data { get; set; }
        public RequesterPostViewModel DriverData { get; set; }
        public VehicleListViewModel VehicleData { get; set; }
        public List<RequesterPostViewModel> Passengers { get; set; }
        public List<RequestMeterial> Meterials { get; set; }
        public List<MeterialFileViewModel> MeterialFiles { get; set; } = new List<MeterialFileViewModel>();

        public RequestApprovalViewModel RequestApproval { get; set; }

        public List<DocumentPostViewModel> RequestDocuments { get; set; }
        public List<DocumentPostViewModel> RequesterDocuments { get; set; }
        public List<DocumentPostViewModel> VehicleDocuments { get; set; }
        public List<DocumentPostViewModel> DriverDocuments { get; set; }
        public List<DocumentPostViewModel> PassengerDocuments { get; set; }
        public List<RequestApprovalHistoryViewModel> ApprovalHistory { get; set; }
        public List<VehicleTrackingHistoryViewModel> TrackingHistory { get; set; }
        public List<RequestItemModel> Spares { get; set; }
        public List<RequestItemModel> Assets { get; set; }
        public List<RequestItemModel> Consumables { get; set; }
        public string? Image { get; set; }
        public string? QRCode { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (User.IsInRole("Meterial") || User.IsInRole("Visitor"))
            {
                var request = await _dbContext.GetAsync<Request>(Convert.ToInt32(id));
                if (request != null && request.AddedBy != CurrentUserID)
                {
                    return Redirect("/home");
                }
            }
            if (id != 0)
            {
                Data = await _dbContext.GetAsync<RequestApprovalHeaderView>($@"SELECT RequestID,RequestedDate,BranchName,SubBranchName,RequestedSlot,ModeName,RequestedLocationName,RequestedByID,RequestModeID,E.EmployeeName,E.Email,E.ContactNumber,C.CompanyName,D.DesignationName,BranchID,SlotID,M.MeterialTypeName,R.ContainsExplosive,R.ContainsCarryItem, R.IsIn, R.IsDisposalRequired, R.MeterialTypeID,R.StatusID,R.HostEmail,R.RequestNo,R.Narration,IsProjectAsset,PONumber,PODate,DeliveryDate,POOwnerID
                FROM viRequest R
                LEFT JOIN Employee E on E.EmployeeID = R.EmployeeID
                LEFT JOIN Company C on C.CompanyID = E.CompanyID
                LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
			    LEFT JOIN RequestMeterialType M on M.MeterialTypeID=R.MeterialTypeID
                Where R.RequestID=@RequestID", new { RequestID = id });

                string LocationName = await _dbContext.GetAsync<string>(
    @"SELECT L.LocationName FROM Request R Left join Location L on L.LocationID = R.StorageLocationID WHERE RequestID = " + id,
    string.Empty);

                if (!string.IsNullOrEmpty(LocationName))
                {
                    Data.LocationName = LocationName != null ? LocationName : "";
                }

                DriverData = await _commonRepository.GetDriverDetailsAsync(id);
                VehicleData = await _commonRepository.GetVehicleDetailsAsync(id);
                Passengers = await _commonRepository.GetPassengersAsync(id);
                if (Data.IsProjectAsset)
                {
                    var meterials = await _dbContext.GetEnumerableAsync<RequestItemModel>($@"Select R.*,I.Name,I.Code,I.PurchaseUnit as Unit,I.IsExpirable,I.Type
                    From RequestItem R
                    JOIN ItemMaster I on I.ItemID=R.ItemID
                    Where R.RequestID=@RequestID and R.IsDeleted=0", new
                    {
                        RequestID = id
                    });

                    Spares = meterials.Where(s => s.Type == "Spares").ToList();
                    Assets = meterials.Where(s => s.Type == "Assets").ToList();
                    Consumables = meterials.Where(s => s.Type == "Consumables").ToList();
                }
                else
                {
                    Meterials = (await _dbContext.GetAllAsyncByFieldName<RequestMeterial>("RequestID", id.ToString())).ToList();

                    MeterialFiles = (await _dbContext.GetEnumerableAsync<MeterialFileViewModel>($@"Select MM.MeterialMediaID,FileName,RequestID
                    From RequestMeterialMedia MM
                    JOIN Medias M on MM.MediaID=M.MediaID
                    Where MM.RequestID=@RequestID and MM.IsDeleted=0", new
                    {
                        RequestID = id
                    })).ToList();
                }
                ApprovalHistory = await _commonRepository.GetRequestApprovalHistory(id, GetClientTimeZone(_httpContextAccessor));
                TrackingHistory = await _commonRepository.GetVehicleTrackingHistory(id, GetClientTimeZone(_httpContextAccessor));
              
              
                if (Data.StatusID==(int)RequestStatus.Accepted)
                {
                    var qrcode = await _dbContext.GetAsync<string>($@"Select E.QRCode
                        From viRequest R
                        JOIN RequestVehicle D on D.RequestID=R.RequestID
                        JOIN Employee E on E.EmployeeID=D.EmployeeID
                        Where R.RequestID=@RequestID", new { RequestID = id });
                    QRCode = qrcode;
                    Image = _mediaRepository.GetQRImage(qrcode);
                    
                }

                var Documents = await _commonRepository.GetAllDocumentsAsync(Convert.ToInt32(id));
                RequestDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Request).ToList();
                RequesterDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Requester).ToList();
                VehicleDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Vehicle).ToList();
                DriverDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Driver).ToList();
                PassengerDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Passenger).ToList();

                RequestApproval = await _dbContext.GetAsync<RequestApprovalViewModel>($@"SELECT RequestID, Date, SlotID, LocationID,StorageLocationID, StatusID, Remarks
                FROM viRequest
                Where RequestID=@RequestID", new
                {
                    RequestID = id
                });

                ViewData["PackingTypes"] = new SelectList((await _dbContext.GetAllAsync<PackingType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.PackingTypeID), Value = s.PackingTypeName }), "ID", "Value");
                var requestMode = await _dbContext.GetAsync<RequestMode>(Data.RequestModeID);
                ViewData["Locations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == requestMode.LocationTypeID).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");
                ViewData["StorageLocations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == (int)LocationTypes.Storage).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");
                ViewData["Slots"] = new SelectList((await GetSlots()).ToList(), "ID", "Value");

                ViewData["POOwners"] = new SelectList((await _dbContext.GetAllAsync<POOwner>()).ToList(), "POOwnerID", "StaffName");
            }

            //Meterials = Meterials ?? new List<RequestMeterial>();
            //ApprovalHistory = ApprovalHistory ?? new List<RequestApprovalHistoryViewModel>();
            //TrackingHistory = TrackingHistory ?? new List<VehicleTrackingHistoryViewModel>();
            //Data = Data ?? new RequestApprovalHeaderView();
            //RequestApproval = RequestApproval ?? new RequestApprovalViewModel();
            //RequesterDocuments = RequesterDocuments ?? new List<DocumentPostViewModel>();
            //DriverDocuments = DriverDocuments ?? new List<DocumentPostViewModel>();
            return Page();
        }


        private async Task<IEnumerable<IdnValuePair>> GetSlots()
        {
            return await _dbContext.GetEnumerableAsync<IdnValuePair>($@"Select SlotID as ID,SlotName as Value
            From viSlot
            Where Date = @Date and BranchID = {Data.BranchID} and AvailableCount>=Case When SlotID={Data.SlotID} then 0 else 1 end", RequestApproval);
        }


        private async Task<int> GetCurrentApprovalStage()
        {
            return await _dbContext.GetAsync<int>($@"SELECT  S.Stage
                    FROM viRequest R
                    LEFT JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID
                    Where R.RequestID=@RequestID and S.UserTypeID = @CurrentUserTypeID", new { RequestID= RequestApproval.RequestID, CurrentUserTypeID= CurrentUserTypeID });

        }
    }
}