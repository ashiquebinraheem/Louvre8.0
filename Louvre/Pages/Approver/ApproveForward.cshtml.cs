using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Security-Control-Room, Security-Duty-Manager")]
    [BindProperties]
    public class ApproveForwardModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IErrorLogRepository _errorLogRepo;

        public ApproveForwardModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _errorLogRepo = errorLogRepo;
        }

        public RequestApprovalHeaderView Data { get; set; }
        public List<RequestMeterial> Meterials { get; set; }
        public List<MeterialFileViewModel> MeterialFiles { get; set; } = new List<MeterialFileViewModel>();
        public RequestVehicle Vehicles { get; set; }

        public List<DocumentPostViewModel> Documents { get; set; }
        public int RequestStatusID { get; set; }
        public List<RequesterPostViewModel> Passengers { get; set; }

        public string? Remarks { get; set; }

        public List<RequestItemModel> Spares { get; set; }
        public List<RequestItemModel> Assets { get; set; }
        public List<RequestItemModel> Consumables { get; set; }

        public async Task OnGetAsync(int id)
        {
            Data = await _dbContext.GetAsync<RequestApprovalHeaderView>($@"SELECT RequestID,RequestedDate,BranchName,SubBranchName,RequestedSlot,ModeName,RequestedLocationName,RequestedByID,RequestModeID,E.EmployeeName,E.Email,E.ContactNumber,C.CompanyName,D.DesignationName,BranchID,SlotID,M.MeterialTypeName,R.ContainsExplosive, R.IsIn, R.IsDisposalRequired, R.Remarks,IsProjectAsset,PONumber,PODate,DeliveryDate,POOwnerID
            FROM viRequest R
            LEFT JOIN Employee E on E.EmployeeID = R.EmployeeID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
			LEFT JOIN RequestMeterialType M on M.MeterialTypeID=R.MeterialTypeID
            Where R.RequestID=@RequestID",new { RequestID =id});
            int requestedId = Convert.ToInt32(Data.RequestedByID);

            var employees = await _commonRepository.GetEmployeesAsync(requestedId);
            ViewData["EmployeesSelectList"] = new SelectList(employees.Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.EmployeeID), Value = s.EmployeeName }), "ID", "Value");
            ViewData["PackingTypes"] = new SelectList((await _dbContext.GetAllAsync<PackingType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.PackingTypeID), Value = s.PackingTypeName }), "ID", "Value");
            ViewData["Vehicles"] = new SelectList((await _dbContext.GetAllAsyncByFieldName<Vehicle>("AddedBy", requestedId.ToString())).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.VehicleID), Value = s.RegisterNo }), "ID", "Value");


            var requestMode = await _dbContext.GetAsync<RequestMode>(Data.RequestModeID);
            ViewData["Locations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == requestMode.LocationTypeID).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");
            ViewData["StorageLocations"] = new SelectList((await _dbContext.GetAllAsync<Location>()).ToList().Where(l => l.LocationTypeID == (int)LocationTypes.Storage).Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.LocationID), Value = s.LocationName }), "ID", "Value");


            if (Data.IsProjectAsset)
            {
                var meterials = await _dbContext.GetEnumerableAsync<RequestItemModel>($@"Select R.*,I.Name,I.Code,I.PurchaseUnit as Unit,I.IsExpirable,I.Type
                    From RequestItem R
                    JOIN ItemMaster I on I.ItemID=R.ItemID
                    Where R.RequestID=@RequestID and R.IsDeleted=0", new { RequestID =id});

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

            Vehicles = await _dbContext.GetAsyncByFieldName<RequestVehicle>("RequestID", id.ToString());
            Documents = await _commonRepository.GetAllDocumentsAsync(Convert.ToInt32(Data.RequestID));
            RequestStatusID = await _dbContext.ExecuteScalarAsync<int>($"Select StatusID From viRequest where RequestID={Data.RequestID}",null);

            ViewData["Slots"] = new SelectList((await GetSlots()).ToList(), "ID", "Value");

            Passengers = await _commonRepository.GetPassengersAsync(id);
            ViewData["POOwners"] = new SelectList((await _dbContext.GetAllAsync<POOwner>()).ToList(), "POOwnerID", "StaffName");
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            BaseResponse result = new BaseResponse();
            try
            {
                var requestApproval = await _dbContext.GetAsync<RequestApproval>($@"SELECT TOP (1) *
                FROM RequestApproval
                WHere RequestID=@RequestID
                Order by RequestApprovalID desc", new { RequestID =Data.RequestID});

                requestApproval.RequestApprovalID = null;
                requestApproval.IsReported = false;
                requestApproval.NeedHigherLevelApproval = false;
                requestApproval.Remarks = Remarks;
                await _dbContext.SaveAsync(requestApproval);
                result.CreatSuccessResponse(105);
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
                var requestApproval = await _dbContext.GetAsync<RequestApproval>($@"SELECT TOP (1) *
                FROM RequestApproval
                WHere RequestID=@RequestID
                Order by RequestApprovalID desc", new { RequestID = Data.RequestID });
                requestApproval.IsReported = false;
                requestApproval.IsRejected = true;
                requestApproval.RequestApprovalID = null;
                requestApproval.NeedHigherLevelApproval = false;
                requestApproval.Remarks = Remarks;
                await _dbContext.SaveAsync(requestApproval);
                result.CreatSuccessResponse(104);
            }
            catch (Exception err)
            {
                result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
            }
            return new JsonResult(result);
        }

        private async Task<IEnumerable<IdnValuePair>> GetSlots()
        {
            return await _dbContext.GetEnumerableAsync<IdnValuePair>($@"Select SlotID as ID,SlotName as Value
            From viSlot
            Where SlotID=@SlotID", new { SlotID = Data.SlotID });
        }
    }
}