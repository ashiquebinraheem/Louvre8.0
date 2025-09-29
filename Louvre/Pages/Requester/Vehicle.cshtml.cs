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
    [Authorize(Roles = "Company,Individual")]
    [BindProperties]
    public class VehicleModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public VehicleModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _errorLogRepo = errorLogRepo;
        }

        public Vehicle Data { get; set; }

        public List<IFormFile>? DocumentFiles { get; set; }
        public List<IFormFile>? DocumentFiles2 { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }
        public async Task OnGetAsync(int? id)
        {
            ViewData["VehicleTypes"] = new SelectList((await _dbContext.GetAllAsync<VehicleType>()).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.VehicleTypeID), Value = s.VehicleTypeName }), "ID", "Value");
            ViewData["VehicleMakes"] = await GetSelectList<VehicleMake>(_dbContext, "VehicleMakeName");
            ViewData["VehiclePlateSources"] = await GetSelectList<VehiclePlateSource>(_dbContext, "VehiclePlateSourceName");
            ViewData["VehiclePlateCategories"] = await GetSelectList<VehiclePlateCategory>(_dbContext, "VehiclePlateCategoryName");
            ViewData["VehiclePlateTypes"] = await GetSelectList<VehiclePlateType>(_dbContext, "VehiclePlateTypeName");

            if (id != null)
            {
                Data = await _dbContext.GetAsync<Vehicle>(Convert.ToInt32(id));
                if (Data.AddedBy != CurrentUserID)
                    Data = null;
                else
                    Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Vehicle, Convert.ToInt32(Data.VehicleID));
            }

            if (Documents == null)
                Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Vehicle,0); ;

            ViewData["Today"] = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            #region Rate Limiting

            var rateLimitCnt = await _dbContext.GetAsync<int>($@"Select Count(*) From Vehicle 
            Where ISNULL(IsDeleted,0)=0 and AddedBy={CurrentUserID} and AddedOn>DATEADD(MINUTE,-2,GETUTCDATE())", null);

            if (rateLimitCnt > 0)
            {
                BaseResponse r = new BaseResponse();
                r.CreatErrorResponse("You’ve already added an vehicle recently. Please wait at least 2 minutes before submitting another one.", "Duplicate Request");
                return new JsonResult(r);
            }

            #endregion

            var isExist = await _dbContext.GetAsyncByFieldName<Vehicle>("RegisterNo", Data.RegisterNo);
            if (isExist != null && isExist.VehicleID != Data.VehicleID && isExist.AddedBy == CurrentUserID)
            {
                var response = new BaseResponse(-105);
                return new JsonResult(response);
            }

            BaseResponse result = new BaseResponse();
            if (cn.State != ConnectionState.Open)   //   // Modified by Abdul Razack for  7: Missing Rate Limiting on Core Business Functions Leading to Data Pollution
                cn.Open();

            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    var id = Convert.ToInt32(await _dbContext.SaveAsync(Data, tran));

                    var documents = new List<Document>();
                    int j = 0, k = 0;

                    foreach (var doc in Documents)
                    {
                        if (!string.IsNullOrEmpty(doc.DocumentNumber))
                        {
                            var document = new Document
                            {
                                DocumentID = doc.DocumentID,
                                DocumentTypeID = doc.DocumentTypeID,
                                ExpiresOn = doc.ExpiresOn,
                                DocumentNumber = doc.DocumentNumber,
                                MediaID = doc.MediaID,
                                MediaID2 = doc.MediaID2,
                            };

                            if (doc.HasFile && j < DocumentFiles.Count)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(doc.MediaID, DocumentFiles[j], "vehicle_documents", id + "_" + doc.DocumentTypeID, tran);
                                document.MediaID = mediaResult.IsSuccess ? mediaResult.MediaID : (int?)null;
                                j++;
                            }

                            if (doc.HasFile2 && k < DocumentFiles2.Count)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(doc.MediaID2, DocumentFiles2[k], "vehicle_documents", id + "2_" + doc.DocumentTypeID, tran);
                                document.MediaID2 = mediaResult.IsSuccess ? mediaResult.MediaID : (int?)null;
                                k++;
                            }

                            documents.Add(document);
                        }
                    }

                    await _dbContext.SaveSubListAsync(documents, "VehicleID", id, tran);

                    tran.Commit();
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
                    result = await _errorLogRepo.CreatThrowResponse(err.Message, CurrentUserID);
                }

                return new JsonResult(result);
            }
        }


        public async Task<IActionResult> OnPostDeleteAsync()
        {
            BaseResponse result = new BaseResponse();
            await _dbContext.DeleteAsync<Vehicle>(Convert.ToInt32(Data.VehicleID));
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }

    }
}
