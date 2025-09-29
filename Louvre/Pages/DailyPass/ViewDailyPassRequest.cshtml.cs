using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Models.Enum;
using Louvre.Shared.Repository;

namespace Louvre.Pages.DailyPass
{
    [Authorize]
    [BindProperties]
    public class ViewDailyPassRequestModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly ICommonRepository _commonRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediaRepository _mediaRepository;

		public ViewDailyPassRequestModel(IDbContext dbContext, ICommonRepository commonRepository, IHttpContextAccessor httpContextAccessor, IMediaRepository mediaRepository)
		{
			_dbContext = dbContext;
			_commonRepository = commonRepository;
			_httpContextAccessor = httpContextAccessor;
			_mediaRepository = mediaRepository;
		}

		public DailyPassRequestApprovalHeaderView Data { get; set; }
        public RequesterPostViewModel DriverData { get; set; }
        public VehicleListViewModel VehicleData { get; set; }
        public List<RequesterPostViewModel> Passengers { get; set; }
        public List<RequestMeterial> Meterials { get; set; }
        public List<MeterialFileViewModel> MeterialFiles { get; set; } = new List<MeterialFileViewModel>();

        public RequestApprovalViewModel RequestApproval { get; set; }

        public List<DocumentPostViewModel> RequesterDocuments { get; set; }
        public List<DocumentPostViewModel> VehicleDocuments { get; set; }
        public List<DocumentPostViewModel> DriverDocuments { get; set; }
        public List<DocumentPostViewModel> PassengerDocuments { get; set; }
        public List<RequestApprovalHistoryViewModel> ApprovalHistory { get; set; }
        public List<VehicleTrackingHistoryViewModel> TrackingHistory { get; set; }
        public string? Image { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (User.IsInRole("Meterial") || User.IsInRole("Visitor"))
            {
                var request = await _dbContext.GetAsync<DailyPassRequest>(Convert.ToInt32(id));
                if (request != null && request.AddedBy != CurrentUserID)
                {
                    return Redirect("/home");
                }
            }
            if (id != 0)
            {
                Data = await _dbContext.GetAsync<DailyPassRequestApprovalHeaderView>($@"SELECT DailyPassRequestID,BranchName,StatusID,SubBranchName,FromDate,ToDate,ModeName,RequestedLocationName,RequestedByID,RequestModeID,E.EmployeeName,E.Email,E.ContactNumber,C.CompanyName,D.DesignationName,BranchID,M.MeterialTypeName,R.ContainsExplosive, R.IsIn, R.IsDisposalRequired, R.MeterialTypeID,R.HostEmail,R.RequestNo,R.Narration
                    FROM viDailyPassRequest R
                    LEFT JOIN Employee E on E.EmployeeID = R.EmployeeID
                    LEFT JOIN Company C on C.CompanyID = E.CompanyID
                    LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
			        LEFT JOIN RequestMeterialType M on M.MeterialTypeID=R.MeterialTypeID
                    Where R.DailyPassRequestID=@DailyPassRequestID", new
                {
                    DailyPassRequestID = id
                });


                if (Data.StatusID == (int)RequestStatus.Accepted)
                {
                    var qrcode = await _dbContext.GetAsync<string>($@"Select E.QRCode
			        From viDailyPassRequest R
					JOIN Employee RQ on RQ.EmployeeID=R.EmployeeID
			        JOIN Employee E on E.EmployeeID=R.DriverID
                    Where R.DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = id });

                    Image = _mediaRepository.GetQRImage(qrcode);
                }


                DriverData = await _commonRepository.GetDailyPassDriverDetailsAsync(id);
                VehicleData = await _commonRepository.GetDailyPassVehicleDetailsAsync(id);
                Passengers = await _commonRepository.GetDailyPassPassengersAsync(id);
                Meterials = (await _dbContext.GetAllAsyncByFieldName<RequestMeterial>("DailyPassRequestID", id.ToString())).ToList();
                ApprovalHistory = await _commonRepository.GetDailyPassRequestApprovalHistory(id, GetClientTimeZone(_httpContextAccessor));
                TrackingHistory = await _commonRepository.GetDailyPassVehicleTrackingHistory(id, GetClientTimeZone(_httpContextAccessor));

                var Documents = await _commonRepository.GetDailyPassAllDocumentsAsync(Convert.ToInt32(Data.DailyPassRequestID));
                RequesterDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Requester).ToList();
                VehicleDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Vehicle).ToList();
                DriverDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Driver).ToList();
                PassengerDocuments = Documents.Where(s => s.DocumentOf == (int)DocumentOf.Passenger).ToList();

                RequestApproval = await _dbContext.GetAsync<RequestApprovalViewModel>($@"SELECT DailyPassRequestID, FromDate, LocationID,StorageLocationID, StatusID, Remarks
                FROM viDailyPassRequest
                Where DailyPassRequestID=@DailyPassRequestID", new
                {
                    DailyPassRequestID = id
                });

                MeterialFiles = (await _dbContext.GetEnumerableAsync<MeterialFileViewModel>($@"Select MM.MeterialMediaID,FileName,RequestID
                    From RequestMeterialMedia MM
                    JOIN Medias M on MM.MediaID=M.MediaID
                    Where MM.DailyPassRequestID=@DailyPassRequestID and MM.IsDeleted=0", new { DailyPassRequestID =id})).ToList();

                ViewData["PackingTypes"] = new SelectList((await _dbContext.GetAllAsync<PackingType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.PackingTypeID), Value = s.PackingTypeName }), "ID", "Value");
                var requestMode = await _dbContext.GetAsync<RequestMode>(Data.RequestModeID);
                ViewData["Locations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == requestMode.LocationTypeID).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");
                ViewData["StorageLocations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == (int)LocationTypes.Storage).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");
            }
            return Page();
        }

        private async Task<int> GetCurrentApprovalStage()
        {
            return await _dbContext.GetAsync<int>($@"SELECT  S.Stage
                    FROM viDailyPassRequest R
                    LEFT JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID
                    Where R.DailyPassRequestID=@DailyPassRequestID and S.UserTypeID =@UserTypeID",
                    new { DailyPassRequestID = RequestApproval.RequestID, UserTypeID = CurrentUserTypeID });

        }
    }
}
