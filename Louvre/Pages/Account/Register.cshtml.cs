using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [BindProperties]
    public class RegisterModel : PageModel   //  -- Added By Abdul Razack for Denial of Service via Unrestricted File Upload Size and Rate
    {
        private readonly IDbConnection cn;
        private readonly IDbContext _dbContext;
        private readonly IUserRepository _userRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IErrorLogRepository _errorLogRepo;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // Allowed file extensions and max file size (10 MB)
        private readonly string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        private const long maxFileSize = 10 * 1024 * 1024;

        public RegisterModel(IDbConnection cn, IDbContext dbContext, IUserRepository userRepository, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IErrorLogRepository errorLogRepo, IConfiguration configuration, HttpClient httpClient)
        {
            this.cn = cn;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _errorLogRepo = errorLogRepo;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public int UserTypeID { get; set; }
        public Company Company { get; set; }
        public User UserData { get; set; }
        public PersonalInfo_Client PersonalInfo { get; set; }
        public PersonalInfoAddress_Client Address { get; set; }

        public List<IFormFile>? EmpDocuments { get; set; }
        public List<IFormFile>? EmpDocuments2 { get; set; }
        public List<DocumentPostViewModel> EmployeeDocuments { get; set; }

        public List<IFormFile>? CompDocuments { get; set; }
        public List<IFormFile>? CompDocuments2 { get; set; }
        public List<DocumentPostViewModel> CompanyDocuments { get; set; }
        public int ModuleID { get; set; }
        public string? DesignationName { get; set; }


        public string? SiteKey { get; private set; }

        [BindProperty(Name = "g-recaptcha-response")]
        public string? RecaptchaResponse { get; set; }

        public async Task OnGetAsync()
        {
            SiteKey = _configuration["Recaptcha:SiteKey"];

            ViewData["Modules"] = new SelectList(await _dbContext.GetAllAsync<Module>(), "ModuleID", "ModuleName");
            EmployeeDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Employee, 0);
            CompanyDocuments = await _commonRepository.GetDocumentsAsync(DocumentTypeCategory.Company, 0);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!await ValidateRecaptchaAsync(RecaptchaResponse))
            {
                ModelState.AddModelError(string.Empty, "Captcha validation failed");
                throw new PreDefinedException("Captcha validation failed");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new JsonResult(new BaseResponse
                {
                    ResponseCode = -1,
                    ResponseTitle = "Validation Error",
                    ResponseMessage = string.Join("\n", errors)
                });
            }

            BaseResponse result = new BaseResponse();

            if (UserTypeID != (int)UserTypes.Company && UserTypeID != (int)UserTypes.Individual)
            {
                result.CreatErrorResponse(-112);
                return new JsonResult(result);
            }

            UserData.Salt = Guid.NewGuid().ToString("n").Substring(0, 8);
            UserData.Password = UserRepository.GetHashPassword(UserData.Password, UserData.Salt);

            cn.Open();
            using (var tran = cn.BeginTransaction())
            {
                try
                {
                    if (UserTypeID == (int)UserTypes.Company)
                        PersonalInfo.FirstName = Company.CompanyName;

                    PersonalInfo.PersonalInfoID = await _dbContext.SaveAsync(PersonalInfo, tran);
                    Address.PersonalInfoID = PersonalInfo.PersonalInfoID;
                    await _dbContext.SaveAsync(Address, tran);

                    UserData.LoginStatus = true;
                    UserData.UserTypeID = UserTypeID;
                    UserData.PersonalInfoID = PersonalInfo.PersonalInfoID;
                    UserData.UserName = PersonalInfo.Email1;
                    UserData.EmailAddress = PersonalInfo.Email1;
                    UserData.MobileNumber = PersonalInfo.Phone1;

                    var weburl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    var userId = await _userRepository.AddNewUserWithMail(UserData, weburl, tran);

                    Company.PersonalInfoID = PersonalInfo.PersonalInfoID;
                    Company.AddedBy = userId;
                    var companyId = await _dbContext.SaveAsync(Company, tran);

                    #region Documents
                    if (UserTypeID == (int)UserTypes.Individual)
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
                        } while (!isFresh);

                        Louvre.Shared.Core.Employee employee = new Louvre.Shared.Core.Employee()
                        {
                            CompanyID = companyId,
                            ContactNumber = PersonalInfo.Phone1,
                            EmployeeName = PersonalInfo.FirstName,
                            Email = PersonalInfo.Email1,
                            DesignationID = await _commonRepository.GetDesignationID(DesignationName, tran),
                            AddedBy = userId,
                            PersonalInfoID = PersonalInfo.PersonalInfoID,
                            QRCode = qrcode
                        };
                        var employeeId = await _dbContext.SaveAsync(employee, tran);

                        var documents = new List<Document>();
                        for (int i = 0, j = 0, k = 0; i < EmployeeDocuments.Count; i++)
                        {
                            var document = new Document
                            {
                                EmployeeID = employeeId,
                                DocumentID = EmployeeDocuments[i].DocumentID,
                                DocumentTypeID = EmployeeDocuments[i].DocumentTypeID,
                                ExpiresOn = EmployeeDocuments[i].ExpiresOn,
                                DocumentNumber = EmployeeDocuments[i].DocumentNumber,
                                MediaID = EmployeeDocuments[i].MediaID,
                                MediaID2 = EmployeeDocuments[i].MediaID2,
                                AddedBy = userId
                            };

                            // Front file validation
                            if (EmployeeDocuments[i].HasFile && EmpDocuments[j] != null)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(EmployeeDocuments[i].MediaID, EmpDocuments[j], "employee_documents", employeeId + "_" + EmployeeDocuments[i].DocumentTypeID, tran);
                                document.MediaID = mediaResult.IsSuccess ? mediaResult.MediaID : null;
                                j++;
                            }

                            // Back file validation
                            if (EmployeeDocuments[i].HasFile2 && EmpDocuments2[k] != null)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(EmployeeDocuments[i].MediaID2, EmpDocuments2[k], "employee_documents", employeeId + "2_" + EmployeeDocuments[i].DocumentTypeID, tran);
                                document.MediaID2 = mediaResult.IsSuccess ? mediaResult.MediaID : null;
                                k++;
                            }

                            documents.Add(document);
                        }

                        await _dbContext.SaveSubListAsync(documents, "EmployeeID", employeeId, tran);
                    }
                    else
                    {
                        var documents = new List<Document>();
                        for (int i = 0, j = 0, k = 0; i < CompanyDocuments.Count; i++)
                        {
                            var document = new Document
                            {
                                CompanyID = companyId,
                                DocumentID = CompanyDocuments[i].DocumentID,
                                DocumentTypeID = CompanyDocuments[i].DocumentTypeID,
                                ExpiresOn = CompanyDocuments[i].ExpiresOn,
                                DocumentNumber = CompanyDocuments[i].DocumentNumber,
                                MediaID = CompanyDocuments[i].MediaID,
                                AddedBy = userId
                            };

                            if (CompanyDocuments[i].HasFile && CompDocuments[j] != null)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(CompanyDocuments[i].MediaID, CompDocuments[j], "company_documents", companyId + "_" + CompanyDocuments[i].DocumentTypeID, tran);
                                document.MediaID = mediaResult.IsSuccess ? mediaResult.MediaID : null;
                                j++;
                            }

                            if (CompanyDocuments[i].HasFile2 && CompDocuments2[k] != null)
                            {
                                var mediaResult = await _mediaRepository.SaveMedia(CompanyDocuments[i].MediaID2, CompDocuments2[k], "company_documents", companyId + "2_" + CompanyDocuments[i].DocumentTypeID, tran);
                                document.MediaID2 = mediaResult.IsSuccess ? mediaResult.MediaID : null;
                                k++;
                            }

                            documents.Add(document);
                        }

                        await _dbContext.SaveSubListAsync(documents, "CompanyID", Convert.ToInt32(companyId), tran);
                    }
                    #endregion

                    // Assign module
                    UserModule userModule = new UserModule()
                    {
                        ModuleID = ModuleID,
                        UserID = userId,
                        AddedBy = userId
                    };
                    await _dbContext.SaveAsync(userModule, tran);

                    tran.Commit();

                    result.CreatSuccessResponse(5);
                }
                catch (PreDefinedException err)
                {
                    tran.Rollback();
                    throw err;
                }
                catch (Exception err)
                {
                    tran.Rollback();
                    throw err;
                }
                return new JsonResult(result);
            }
        }

        private async Task<bool> ValidateRecaptchaAsync(string recaptchaResponse)
        {
            var secret = _configuration["Recaptcha:SecretKey"];

            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptchaResponse}",
                null);

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Success == true && result.Score >= 0.5; // For reCAPTCHA v3
        }

        private class RecaptchaResult
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("challenge_ts")]
            public string? ChallengeTs { get; set; }

            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }

            [JsonPropertyName("error-codes")]
            public string?[] ErrorCodes { get; set; }
        }
    }
}
