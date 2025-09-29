using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Progbiz.API.Controllers;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Louvre.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IDbConnection cn;
        private readonly IDbContext _dbContext;
        private readonly IUserRepository _userRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AccountController(IDbConnection cn, IDbContext dbContext, IUserRepository userRepository, ICommonRepository commonRepository, IMediaRepository mediaRepository, IEmailSender emailSender, IConfiguration configuration)
        {
            this.cn = cn;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _commonRepository = commonRepository;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        //[HttpGet("get-initial-data")]
        //public async Task<RegistrationInitialDataModel> GetInitialData()
        //{
        //    var weburl = _configuration["MainDomain"];

        //    var result = new RegistrationInitialDataModel
        //    {
        //        DocumentTypes = (await _dbContext.GetIdValuePairAsync<DocumentType>("DocumentTypeName", $"DocumentTypeCategoryID=@DocumentTypeCategoryID", new { DocumentTypeCategoryID = (int)DocumentTypeCategory.Employee })).ToList()
        //    };
        //    return result;
        //}


        //[HttpPost("register")]
        //public async Task<APIBaseResponse> Register(RegisterPostModel model)
        //{
        //    APIBaseResponse result = new APIBaseResponse();

        //    var hostDetails = await _dbContext.GetAsync<User>($@"SELECT  Top(1) *
        //    FROM  Users
        //    Where ISNULL(IsDeleted,0)=0 and UserTypeID not in ({(int)UserTypes.Company},{(int)UserTypes.Individual}) and (EmailAddress=@HostDetail or MobileNumber=@HostDetail)",model);

        //    if (hostDetails == null)
        //    {
        //        result.CreateFailureResponse("Employee not found!!");
        //        return result;
        //    }

        //    var weburl = _configuration["MainDomain"];


        //    cn.Open();
        //    using (var tran = cn.BeginTransaction())
        //    {
        //        try
        //        {
        //            PersonalInfo PersonalInfo = new PersonalInfo()
        //            {
        //                Email1 = model.EmailId,
        //                FirstName = model.FullName,
        //                Phone1 = model.PhoneNumber
        //            };
        //            PersonalInfo.PersonalInfoID = await _dbContext.SaveAsync(PersonalInfo, tran);
        //            var salt = Guid.NewGuid().ToString("n").Substring(0, 8);
        //            User UserData = new User()
        //            {
        //                LoginStatus = true,
        //                PersonalInfoID = PersonalInfo.PersonalInfoID,
        //                UserTypeID = (int)UserTypes.Individual,
        //                EmailAddress = PersonalInfo.Email1,
        //                MobileNumber = PersonalInfo.Phone1,
        //                UserName = PersonalInfo.Email1,
        //                HostID = hostDetails.UserID,
        //                Salt = salt,
        //                Password = UserRepository.GetHashPassword(model.Password,salt)
        //            };
        //            UserData.UserID = await _userRepository.AddNewUserWithMail(UserData, weburl, tran);


        //            Company Company = new Company()
        //            {
        //                PersonalInfoID = PersonalInfo.PersonalInfoID,
        //                AddedBy = UserData.UserID,
        //                CompanyName = model.CompanyName
        //            };
        //            Company.CompanyID = await _dbContext.SaveAsync(Company, tran);
              

        //            Core.Employee Employee = new Core.Employee()
        //            {
        //                CompanyID = Company.CompanyID,
        //                ContactNumber = PersonalInfo.Phone1,
        //                EmployeeName = PersonalInfo.FirstName,
        //                Email = PersonalInfo.Email1,
        //                AddedBy = UserData.UserID,
        //                PersonalInfoID = PersonalInfo.PersonalInfoID,
        //                DesignationID= await _commonRepository.GetDesignationID(model.DesignationName, tran),
        //            };
        //            bool isFresh = false;
        //            do
        //            {
        //                Random random = new Random();
        //                Employee.QRCode = random.Next(10000000, 99999999).ToString();
        //                var req = await _dbContext.GetAsyncByFieldName<Employee>("QRCode", Employee.QRCode, tran);
        //                if (req == null)
        //                    isFresh = true;
        //            } while (isFresh == false);

        //            Employee.EmployeeID = await _dbContext.SaveAsync(Employee, tran);

        //            if (model.Documents != null)
        //            {
        //                foreach (var document in model.Documents)
        //                {
        //                    var doc = new Document();
        //                    doc.DocumentNumber = document.DocumentNo;
        //                    doc.DocumentTypeID = document.DocumentType;
        //                    doc.ExpiresOn = document.DocumentExpiry;
        //                    doc.EmployeeID = Employee.EmployeeID;
        //                    if (document.MediaID != 0)
        //                        doc.MediaID = document.MediaID;
        //                    if (document.MediaID2 != 0)
        //                        doc.MediaID2 = document.MediaID2;
        //                    await _dbContext.SaveAsync(doc, tran);
        //                }
        //            }

        //            UserModule userModule = new UserModule()
        //            {
        //                ModuleID = model.Type,
        //                UserID = UserData.UserID,
        //                AddedBy = UserData.UserID
        //            };
        //            await _dbContext.SaveAsync(userModule, tran);

        //            tran.Commit();

        //            #region Mail

        //            var body = $@"Hi,<br/>
        //            You have got new registration request from one of your client {PersonalInfo.FirstName}. Contact with security department and do the needfull.";
        //            await _emailSender.SendHtmlEmailAsync(hostDetails.EmailAddress, $"New Registraion Request From { PersonalInfo.FirstName}", body, tran);

        //            #endregion

        //            result.Message = "Please goto your email inbox and verify your registration";

        //            return result;
        //        }
        //        catch (Exception err)
        //        {
        //            tran.Rollback();
        //            if (err.Message == "-5")
        //            {
        //                result.CreateFailureResponse("A user is already exist with the email address!!");
        //                return result;
        //            }
        //            else
        //            {
        //                result.CreateFailureResponse(err.Message);
        //                return result;
        //            }
        //        }
        //    }
        //}


        //[HttpPost("add-vehicle")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<AddVehicleViewModel> AddVehicle(AddVehiclePostModel model)
        //{
        //    AddVehicleViewModel result = new AddVehicleViewModel();

        //    var isExist = await _dbContext.GetAsyncByFieldName<Vehicle>("RegisterNo", model.RegisterNo);
        //    if (isExist != null && isExist.AddedBy == CurrentUserID)
        //    {
        //        result.CreateFailureResponse("a vehicle already exist with the same register number");
        //        return result;
        //    }
            
        //    cn.Open();
        //    using (var tran = cn.BeginTransaction())
        //    {
        //        try
        //        {
        //            Vehicle vehicle = new Vehicle()
        //            {
        //                RegisterNo = model.RegisterNo
        //            };
        //            var vehicleId = await _dbContext.SaveAsync(vehicle, tran);

        //            tran.Commit();
        //            result.Vehicles = (await _dbContext.GetEnumerableAsync<ViVehicle>($@"Select VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo, VehicleMakeName, 
        //                VehiclePlateSourceName, VehiclePlateTypeName, VehiclePlateCategoryName 
        //                From viVehicle Where AddedBy=@CurrentUserID", new { CurrentUserID })).ToList();
        //            result.NewVehicleID = vehicleId;
        //        }
        //        catch (Exception err)
        //        {
        //            tran.Rollback();
        //            result.CreateFailureResponse(err.Message);
        //        }
        //    }
        //    return result;
        //}


        //[HttpPost("add-co-passenger")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<AddPassendgerViewModel> AddCoPassenger(AddCoPassendgerPostModel model)
        //{
        //    AddPassendgerViewModel result = new AddPassendgerViewModel();

        //    var isExist = await _dbContext.GetAsync<Employee>($"Select * from Employee Where EmployeeName=@EmployeeName and AddedBy ={CurrentUserID}", model);
        //    if (isExist != null)
        //    {
        //        result.CreateFailureResponse("A person already exist with the same name");
        //        return result;
        //    }
            
        //    cn.Open();
        //    using (var tran = cn.BeginTransaction())
        //    {
        //        try
        //        {
        //            bool isFresh = false;
        //            string qrcode;
        //            do
        //            {
        //                Random random = new Random();
        //                qrcode = random.Next(10000000, 99999999).ToString();
        //                var req = await _dbContext.GetAsyncByFieldName<Employee>("QRCode", qrcode, tran);
        //                if (req == null)
        //                    isFresh = true;
        //            } while (isFresh == false);


        //            Employee employee = new Employee()
        //            {
        //                EmployeeName = model.EmployeeName,
        //                CompanyID = await _commonRepository.GetCompanyID(model.CompanyName, CurrentUserID, tran),
        //                DesignationID = await _commonRepository.GetDesignationID(model.DesignationName, tran),
        //                ContactNumber = model.ContactNumber,
        //                Email = model.Email,
        //                QRCode = qrcode
        //            };
        //            var employeeId = await _dbContext.SaveAsync(employee, tran);


        //            if (model.Documents != null)
        //            {
        //                foreach (var document in model.Documents)
        //                {
        //                    var doc = await _dbContext.GetAsync<Document>($"Select * from Document where DocumentTypeID={document.DocumentType} and EmployeeID={employeeId}", null, tran);
        //                    if (doc == null)
        //                    {
        //                        doc = new Document();
        //                    }
        //                    doc.DocumentNumber = document.DocumentNo;
        //                    doc.DocumentTypeID = document.DocumentType;
        //                    doc.ExpiresOn = document.DocumentExpiry;
        //                    doc.EmployeeID = employeeId;
        //                    if (document.MediaID != 0)
        //                        doc.MediaID = document.MediaID;
        //                    if (document.MediaID2 != 0)
        //                        doc.MediaID2 = document.MediaID2;
        //                    await _dbContext.SaveAsync(doc, tran);
        //                }
        //            }

        //            tran.Commit();
        //            result.Passengers = await _commonRepository.GetCopassengers(CurrentUserID);
        //            result.NewPassengerID = employeeId;
        //            result.Message = "Person added successfully";
        //        }
        //        catch (Exception err)
        //        {
        //            tran.Rollback();
        //            result.CreateFailureResponse(err.Message);
        //        }
        //    }
        //    return result;
        //}


        //[HttpPost("remove-vehicle")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<APIBaseResponse> RemoveVehicle(VehicleIDModel model)
        //{
        //    APIBaseResponse result = new ();
        //    await _dbContext.DeleteAsync<Core.Vehicle>(model.VehicleID);
        //    return result;
        //}

        //[HttpPost("remove-co-passenger")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<APIBaseResponse> RemoveCoPassenger(EmployeeIDModel model)
        //{
        //    APIBaseResponse result = new();
        //    await _dbContext.DeleteAsync<Core.Employee>(model.EmployeeID);
        //    return result;
        //}

    }
}
