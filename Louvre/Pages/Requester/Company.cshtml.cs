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
using System.Data;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize(Roles = "Company,Individual")]
    [BindProperties]
    public class CompanyModel : BasePageModel
    {
        // Modified by Abdul Razack for  7: Missing Rate Limiting on Core Business Functions Leading to Data Pollution
        private readonly IDbContext _dbContext;
        private readonly IDbConnection cn;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IErrorLogRepository _errorLogRepo;

        public CompanyModel(IDbContext dbContext, IDbConnection cn, ICommonRepository commonRepository, IMediaRepository mediaRepository, IErrorLogRepository errorLogRepo)
        {
            _dbContext = dbContext;
            this.cn = cn;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _errorLogRepo = errorLogRepo;
        }

        public Louvre.Shared.Core.Company Data { get; set; }

        public List<IFormFile> DocumentFiles { get; set; }
        public List<IFormFile> DocumentFiles2 { get; set; }
        public List<DocumentPostViewModel> Documents { get; set; }
        public async Task OnGetAsync(int? id)
        {
            if (id != null)
            {
                Data = await _dbContext.GetAsync<Louvre.Shared.Core.Company>(Convert.ToInt32(id));
                if (Data.AddedBy != CurrentUserID)
                    Data = null;
                else
                    Documents = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, Convert.ToInt32(Data.CompanyID));
            }

            if (Documents == null)
                Documents = new List<DocumentPostViewModel>();
            ViewData["Today"] = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var companyId = await _commonRepository.GetCompany(Data.CompanyName, CurrentUserID);
            if (companyId.HasValue && companyId.Value != Data.CompanyID)
            {
                var response = new BaseResponse(-106);
                return new JsonResult(response);
            }

            BaseResponse result = new BaseResponse();
            if (cn.State != ConnectionState.Open)
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
                                var mediaResult = await _mediaRepository.SaveMedia(doc.MediaID, DocumentFiles[j], "company_documents", id + "_" + doc.DocumentTypeID, tran);
                                document.MediaID = mediaResult.IsSuccess ? mediaResult.MediaID : (int?)null;
                                j++;
                            }

                            if (doc.HasFile2 && k < DocumentFiles2.Count)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(doc.MediaID2, DocumentFiles2[k], "company_documents", id + "2_" + doc.DocumentTypeID, tran);
                                document.MediaID2 = mediaResult.IsSuccess ? mediaResult.MediaID : (int?)null;
                                k++;
                            }

                            documents.Add(document);
                        }
                    }

                    await _dbContext.SaveSubListAsync(documents, "CompanyID", id, tran);

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
            }

            return new JsonResult(result); // ✅ fixed missing return
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            BaseResponse result = new BaseResponse();
            await _dbContext.DeleteAsync<Louvre.Shared.Core.Company>(Convert.ToInt32(Data.CompanyID));
            result.CreatSuccessResponse(1);
            return new JsonResult(result);
        }

    }
}
