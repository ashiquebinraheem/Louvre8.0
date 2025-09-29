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
    public class DriverModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public DriverModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _errorLogRepo = errorLogRepo;
        }

        public Louvre.Shared.Core.Employee Data { get; set; }

        public string? DesignationName { get; set; }

        public List<IFormFile>? DocumentFiles { get; set; }
        public List<IFormFile>? DocumentFiles2 { get; set; }

        public List<DocumentPostViewModel> Documents { get; set; }
        public async Task OnGetAsync(int? id)
        {
            ViewData["Companies"] = new SelectList((await _dbContext.GetAllAsyncByFieldName<Louvre.Shared.Core.Company>("AddedBy", CurrentUserID.ToString())).ToList().Select(s => new IdnValuePair() { ID = Convert.ToInt32(s.CompanyID), Value = s.CompanyName }), "ID", "Value");
            ViewData["Designations"] = (await _dbContext.GetAllAsync<EmployeeDesignation>()).ToList().Select(s => s.DesignationName);
            ViewData["Countries"] = await GetSelectList<Country>(_dbContext, "CountryName");

            if (id != null)
            {
                Data = await _dbContext.GetAsync<Louvre.Shared.Core.Employee>(Convert.ToInt32(id));
                if (Data.AddedBy != CurrentUserID)
                    Data = null;
                else
                {
                    Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, Convert.ToInt32(Data.EmployeeID));
                    if(Data.DesignationID!=null)
                        DesignationName = (await _dbContext.GetAsync<EmployeeDesignation>(Convert.ToInt32(Data.DesignationID))).DesignationName;
                }

            }

            if (Documents == null)
                Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, 0);
            ViewData["Today"] = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {

            #region Rate Limiting

            var rateLimitCnt = await _dbContext.GetAsync<int>($@"Select Count(*) From Employee 
            Where ISNULL(IsDeleted,0)=0 and AddedBy={CurrentUserID} and AddedOn>DATEADD(MINUTE,-2,GETUTCDATE())", null);

            if (rateLimitCnt > 0)
            {
                BaseResponse r = new BaseResponse();
                r.CreatErrorResponse("You’ve already added an employee recently. Please wait at least 2 minutes before submitting another one.", "Duplicate Request");
                return new JsonResult(r);
            }

            #endregion


            var isExist = await _dbContext.GetAsyncByFieldName<Louvre.Shared.Core.Employee>("EmployeeName", Data.EmployeeName);
            if (isExist != null && isExist.EmployeeID != Data.EmployeeID && isExist.AddedBy == CurrentUserID)
            {
                var response = new BaseResponse(-104);
                return new JsonResult(response);
            }

            BaseResponse result = new BaseResponse();
            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    if (Data.EmployeeID == null)
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
                        Data.QRCode = qrcode;
                    }

                    Data.DesignationID = await _commonRepository.GetDesignationID(DesignationName, tran);

                    var id = await _dbContext.SaveAsync(Data, tran);

                    #region  Documents

                    var documents = new List<Document>();

                    for (int i = 0, j = 0, k=0; i < Documents.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(Documents[i].DocumentNumber))
                        {
                            var document = new Document
                            {
                                DocumentID = Documents[i].DocumentID,
                                DocumentTypeID = Documents[i].DocumentTypeID,
                                ExpiresOn = Documents[i].ExpiresOn,
                                DocumentNumber = Documents[i].DocumentNumber,
                                MediaID = Documents[i].MediaID,
                                MediaID2 = Documents[i].MediaID2
                            };

                            if (Documents[i].HasFile)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(Documents[i].MediaID, DocumentFiles[j], "employee_documents", id + "_" + Documents[i].DocumentTypeID, tran);
                                if (mediaResult.IsSuccess)
                                {
                                    document.MediaID = mediaResult.MediaID;
                                }
                                else
                                    document.MediaID = null;
                                j++;
                            }

                            if (Documents[i].HasFile2)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(Documents[i].MediaID2, DocumentFiles2[k], "employee_documents", id + "2_" + Documents[i].DocumentTypeID, tran);
                                if (mediaResult.IsSuccess)
                                {
                                    document.MediaID2 = mediaResult.MediaID;
                                }
                                else
                                    document.MediaID2 = null;
                                k++;
                            }

                            documents.Add(document);
                        }
                    }
                    await _dbContext.SaveSubListAsync(documents, "EmployeeID", Convert.ToInt32(id), tran);

                    #endregion

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
            await _dbContext.DeleteAsync<Louvre.Shared.Core.Employee>(Convert.ToInt32(Data.EmployeeID));
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }

    }
}
