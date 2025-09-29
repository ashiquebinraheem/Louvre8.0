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
