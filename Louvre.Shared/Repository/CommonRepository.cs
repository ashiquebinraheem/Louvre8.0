using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Models.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository
{
    public interface ICommonRepository
    {
        #region Common

        List<string> GetHonorifics();
        List<IdnValuePair> GetGenders();
        List<IdnValuePair> GetMaritalStatuses();
        List<IdnValuePair> GetBloodGroups();

        #endregion

        List<IdnValuePair> GetDocumentTypeCategories();
        Task<List<RequesterPostViewModel>> GetEmployeesAsync(int currentUserId, IDbTransaction tran = null);
        Task<List<DocumentPostViewModel>> GetDocumentsAsync(DocumentTypeCategory documentTypeCategoryID, int id, IDbTransaction tran = null);
        Task<List<DocumentPostViewModel>> GetAllDocumentsAsync(int requestId, IDbTransaction tran = null);
        Task<List<DocumentPostViewModel>> GetAllVisitRequestDocumentsAsync(int requestId, IDbTransaction tran = null);
        Task<int> GetCompanyID(string companyName, int addedBy, IDbTransaction tran = null);
        Task<int?> GetCompany(string companyName, int addedBy, IDbTransaction tran = null);
        Task<int?> GetDesignationID(string designationName, IDbTransaction tran = null);
        Task SendVisitorRequestApprovalMail(int requestId, string url, IDbTransaction tran = null);
        Task<List<RequesterPostViewModel>> GetPassengersAsync(int requestId, IDbTransaction tran = null);
        Task<RequesterPostViewModel> GetDriverDetailsAsync(int requestId, IDbTransaction tran = null);
        Task<VehicleListViewModel> GetVehicleDetailsAsync(int requestId, IDbTransaction tran = null);
        Task<List<MeterialViewModel>> GetMeterialsAsync(int requestId, IDbTransaction tran = null);

        Task<List<RequestApprovalHistoryViewModel>> GetRequestApprovalHistory(int requestId, int timeZoneMinutes);
        Task<List<VehicleTrackingHistoryViewModel>> GetVehicleTrackingHistory(int requestId, int timeZoneMinutes);

        Task SendNewRequestMailToApprover(int requestId, string url, IDbTransaction tran = null);
        Task SendNewDailyPassRequestMailToApprover(int requestId, string url, IDbTransaction tran = null);
        Task<MyProfileInfoModel> GetProfileInfo(int CurrentUserID);
        Task<IEnumerable<IdnValuePair>> GetCopassengers(int CurrentUserID, IDbTransaction tran = null);


        Task<RequesterPostViewModel> GetDailyPassDriverDetailsAsync(int requestId, IDbTransaction tran = null);
        Task<VehicleListViewModel> GetDailyPassVehicleDetailsAsync(int requestId, IDbTransaction tran = null);
        Task<List<RequesterPostViewModel>> GetDailyPassPassengersAsync(int requestId, IDbTransaction tran = null);
        Task<List<DocumentPostViewModel>> GetDailyPassAllDocumentsAsync(int requestId, IDbTransaction tran = null);
        Task<List<MeterialViewModel>> GetDailyPassMeterialsAsync(int requestId, IDbTransaction tran = null);
        Task<List<RequestApprovalHistoryViewModel>> GetDailyPassRequestApprovalHistory(int requestId, int timeZoneMinutes);
        Task<List<VehicleTrackingHistoryViewModel>> GetDailyPassVehicleTrackingHistory(int requestId, int timeZoneMinutes);


    }

    public class CommonRepository : ICommonRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IMediaRepository _mediaRepository;
        private readonly IEmailSender _emailSender;

        public CommonRepository(IDbContext entity, IMediaRepository mediaRepository, IEmailSender emailSender)
        {
            _dbContext = entity;
            _mediaRepository = mediaRepository;
            _emailSender = emailSender;
        }



        #region Default Value

        #region Common

        public List<string> GetHonorifics()
        {
            var lst = new List<string>
            {
                "Mr",
                "Mrs",
                "Ms"
            };
            return lst;
        }

        public List<IdnValuePair> GetGenders()
        {
            var lst = new List<IdnValuePair>
            {
                new IdnValuePair(){ ID=1, Value ="Male" },
                new IdnValuePair(){ ID=2, Value ="Female" },
                new IdnValuePair(){ ID=3, Value ="Transgender" },
            };
            return lst;
        }

        public List<IdnValuePair> GetMaritalStatuses()
        {
            var lst = new List<IdnValuePair>
            {
                new IdnValuePair(){ ID=1, Value ="Single" },
                new IdnValuePair(){ ID=2, Value ="Married" },
                new IdnValuePair(){ ID=3, Value ="Seperated" },
                new IdnValuePair(){ ID=4, Value ="Widowed" },
            };
            return lst;
        }

        public List<IdnValuePair> GetBloodGroups()
        {
            var lst = new List<IdnValuePair>
            {
                new IdnValuePair(){ ID=1, Value ="A+" },
                new IdnValuePair(){ ID=2, Value ="A-" },
                new IdnValuePair(){ ID=3, Value ="B+" },
                new IdnValuePair(){ ID=4, Value ="B-" },
                new IdnValuePair(){ ID=5, Value ="O+" },
                new IdnValuePair(){ ID=6, Value ="O-" },
                new IdnValuePair(){ ID=7, Value ="AB+" },
                new IdnValuePair(){ ID=8, Value ="AB-" },
            };
            return lst;
        }

        #endregion

        public List<IdnValuePair> GetDocumentTypeCategories()
        {
            var lst = new List<IdnValuePair>
            {
                new IdnValuePair(){ ID=1, Value ="Employee" },
                new IdnValuePair(){ ID=2, Value ="Company" },
                new IdnValuePair(){ ID=3, Value ="Vehicle" },
                new IdnValuePair(){ ID=4, Value ="Request" },
                new IdnValuePair(){ ID=5, Value ="Visit Request" },
                new IdnValuePair(){ ID=6, Value ="Monthly Pass Request" }
            };
            return lst;
        }

        #endregion

        public async Task<List<RequesterPostViewModel>> GetEmployeesAsync(int currentUserId, IDbTransaction tran = null)
        {
            return (await _dbContext.GetEnumerableAsync<RequesterPostViewModel>($@"SELECT EmployeeID, EmployeeName, DesignationName, CompanyName, Email, ContactNumber, E.CompanyID
            FROM Employee E
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
            Where E.AddedBy = {currentUserId} and ISNULL(E.IsDeleted, 0) = 0", null, tran)).ToList();

        }

        public async Task<List<DocumentPostViewModel>> GetDocumentsAsync(DocumentTypeCategory documentTypeCategoryID, int id, IDbTransaction tran = null)
        {
            string condition = "";
            switch (documentTypeCategoryID)
            {
                case DocumentTypeCategory.Company:
                    condition = $"D.CompanyID={id}";
                    break;
                case DocumentTypeCategory.Employee:
                    condition = $"D.EmployeeID={id}";
                    break;
                case DocumentTypeCategory.Request:
                    condition = $"D.RequestID={id}";
                    break;
                case DocumentTypeCategory.Vehicle:
                    condition = $"D.VehicleID={id}";
                    break;
                case DocumentTypeCategory.VisitRequest:
                    condition = $"D.VisitRequestID={id}";
                    break;
                case DocumentTypeCategory.DailyPassRequest:
                    condition = $"D.DailyPassRequestID={id}";
                    break;
            }

            string query = $@"SELECT Distinct DocumentID,T.DocumentTypeID,ISNULL(T.DocumentTypeName,'') as DocumentTypeName,ISNULL(D.DocumentNumber,'') as DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
            FROM DocumentType T
            LEFT JOIN Document D on T.DocumentTypeID=D.DocumentTypeID and {condition} and D.IsDeleted=0
            LEFT JOIN Medias M on M.MediaID=D.MediaID
            LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
            Where T.DocumentTypeCategoryID={(int)documentTypeCategoryID} and T.ISDeleted=0";


            return (await _dbContext.GetEnumerableAsync<DocumentPostViewModel>(query, null, tran)).ToList();

        }

        public async Task<List<DocumentPostViewModel>> GetAllDocumentsAsync(int requestId, IDbTransaction tran = null)
        {
            string query = $@"SELECT Distinct DocumentID,DocumentTypeID,ISNULL(DocumentTypeName,'') as DocumentTypeName,ISNULL(DocumentNumber,'') as DocumentNumber,FileName,MediaID,ExpiresOn,DocumentOf,MediaID2,FileName2, IsRequired
            From
            (

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Request} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID and D.RequestID={requestId}
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null  and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Requester} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN Request R on R.RequestID={requestId}
                JOIN Employee E on E.EmployeeID=R.EmployeeID
                JOIN Company C on C.CompanyID=E.CompanyID and (D.EmployeeID=E.EmployeeID or C.CompanyID=D.CompanyID)
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Vehicle} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN RequestVehicle RV on RV.RequestID={requestId} and D.VehicleID=RV.VehicleID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Driver} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN RequestVehicle RV on RV.RequestID={requestId} and D.EmployeeID=RV.EmployeeID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName+' ('+E.EmployeeName+')' as DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Passenger} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN RequestPassenger RP on RP.RequestID={requestId} and D.EmployeeID=RP.EmployeeID
                JOIN Employee E on E.EmployeeID=RP.EmployeeID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

            ) As A";

            return (await _dbContext.GetEnumerableAsync<DocumentPostViewModel>(query, null, tran)).ToList();

        }


        public async Task<List<DocumentPostViewModel>> GetDailyPassAllDocumentsAsync(int requestId, IDbTransaction tran = null)
        {
            string query = $@"SELECT Distinct DocumentID,DocumentTypeID,ISNULL(DocumentTypeName,'') as DocumentTypeName,ISNULL(DocumentNumber,'') as DocumentNumber,FileName,MediaID,ExpiresOn,DocumentOf,MediaID2,FileName2, IsRequired
            From
            (

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Request} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID and D.DailyPassRequestID={requestId}
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null  and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Requester} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN DailyPassRequest R on R.DailyPassRequestID={requestId}
                JOIN Employee E on E.EmployeeID=R.EmployeeID
                JOIN Company C on C.CompanyID=E.CompanyID and (D.EmployeeID=E.EmployeeID or C.CompanyID=D.CompanyID)
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Vehicle} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN DailyPassRequest RV on RV.DailyPassRequestID={requestId} and D.VehicleID=RV.VehicleID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Driver} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN DailyPassRequest RV on RV.DailyPassRequestID={requestId} and D.EmployeeID=RV.DriverID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName+' ('+E.EmployeeName+')' as DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,{(int)DocumentOf.Passenger} as DocumentOf,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN RequestPassenger RP on RP.DailyPassRequestID={requestId} and D.EmployeeID=RP.EmployeeID
                JOIN Employee E on E.EmployeeID=RP.EmployeeID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

            ) As A";

            return (await _dbContext.GetEnumerableAsync<DocumentPostViewModel>(query, null, tran)).ToList();

        }

        public async Task<List<DocumentPostViewModel>> GetAllVisitRequestDocumentsAsync(int requestId, IDbTransaction tran = null)
        {
            string query = $@"SELECT Distinct DocumentID,DocumentTypeID,ISNULL(DocumentTypeName,'') as DocumentTypeName,ISNULL(DocumentNumber,'') as DocumentNumber,FileName,MediaID,ExpiresOn,MediaID2,FileName2, IsRequired
            From
            (

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID and D.VisitRequestID={requestId}
                JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN VisitRequest R on R.VisitRequestID={requestId}
                JOIN Employee E on E.EmployeeID=R.EmployeeID
                JOIN Company C on C.CompanyID=E.CompanyID and (D.EmployeeID=E.EmployeeID or C.CompanyID=D.CompanyID)
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

                UNION

                SELECT Distinct DocumentID,T.DocumentTypeID,T.DocumentTypeName,D.DocumentNumber,M.FileName,M.MediaID,D.ExpiresOn,MediaID2,M2.FileName as FileName2, Case When D.MediaID is null and IsRequired=1 then 1 else 0 end IsRequired
                FROM Document D
                JOIN DocumentType T on T.DocumentTypeID=D.DocumentTypeID
                JOIN VisitRequest RV on RV.VisitRequestID={requestId} and D.VehicleID=RV.VehicleID
                LEFT JOIN Medias M on M.MediaID=D.MediaID
                LEFT JOIN Medias M2 on M2.MediaID=D.MediaID2
                Where D.IsDeleted=0 and D.DocumentNumber is not null and T.ISDeleted=0 and (D.MediaID is not null or D.MediaID2 is not null)

            ) As A";

            return (await _dbContext.GetEnumerableAsync<DocumentPostViewModel>(query, null, tran)).ToList();

        }

        public async Task<int> GetCompanyID(string companyName, int addedBy, IDbTransaction tran = null)
        {
            int? companyId = await _dbContext.GetAsync<int?>($@"SELECT  CompanyID
            FROM Company
            Where CompanyName = N'{companyName}' and AddedBy = {addedBy} and ISNULL(IsDeleted, 0) = 0", null, tran);

            if (companyId == null)
            {
                Company company = new Company()
                {
                    CompanyName = companyName
                };
                companyId = await _dbContext.SaveAsync(company, tran);
            }
            return Convert.ToInt32(companyId);
        }

        public async Task<int?> GetCompany(string companyName, int addedBy, IDbTransaction tran = null)
        {
            return await _dbContext.GetAsync<int?>($@"SELECT  CompanyID
            FROM Company
            Where CompanyName = N'{companyName}' and AddedBy = {addedBy} and ISNULL(IsDeleted, 0) = 0", null, tran);
        }

        public async Task<int?> GetDesignationID(string designationName, IDbTransaction tran = null)
        {
            int? designationId = null;
            if (!string.IsNullOrEmpty(designationName))
            {
                designationId = await _dbContext.GetAsync<int?>($@"SELECT  DesignationID
                FROM EmployeeDesignation
                Where DesignationName = N'{designationName}'", null, tran);

                if (designationId == null)
                {
                    EmployeeDesignation company = new EmployeeDesignation()
                    {
                        DesignationName = designationName
                    };
                    designationId = await _dbContext.SaveAsync(company, tran);
                }
            }
            return designationId;
        }


        public async Task SendVisitorRequestApprovalMail(int requestId, string url, IDbTransaction tran = null)
        {

            var requestData = await _dbContext.GetAsync<VisitRequestView>($@"Select * 
                    From viVisitRequest 
                    Where VisitRequestID={requestId}", null, tran);

            var gatepassData = await _dbContext.GetAsync<GatePassViewModel>($@"Select R.QRCode,R.CompanyName,R.EmployeeName,DesignationName,
			        R.ContactNumber,V.PlateNo,VT.VehicleTypeName,VehicleSize,V.RegisterNo
			        From viVisitRequest R
			        JOIN Vehicle V on V.VehicleID=R.VehicleID
			        JOIN Employee E on E.EmployeeID=R.EmployeeID
			        LEFT JOIN EmployeeDesignation ED on ED.DesignationID=E.DesignationID
			        LEFT JOIN Company C on C.CompanyID=E.CompanyID
			        LEFT JOIN VehicleType VT on VT.VehicleTypeID=V.VehicleTypeID
                    Where R.VisitRequestID={requestId}", null, tran);


            var image = _mediaRepository.GetQRImage(requestData.QRCode);
            var msg = $"<img alt = 'Embedded Image' height='50' width='50' src =\"{image}\">";


            var body = $@"<!DOCTYPE html>
					<html>
					<head>
					<title>Your Visit Request Accepted</title>
					<link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
					</head>
					<body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>

					    <div style='width:600px;height:80vh;background-color:#fff;margin: 0 auto;margin-top:50px;-webkit-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-moz-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);-o-box-shadow: 0px 0px 20px rgba(0,0,0,0.1);box-shadow: 0px 0px 20px rgba(0,0,0,0.1);border-radius: 10px;padding: 30px;'>
					
                            <div style='background-color:none;width:50%;float: left;'>
						        <p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Meeting Date:</span> {requestData.MeetingDate}</p>
					        </div>

                            <div style='background-color:none;width:50%;float: left;'>
						        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Duration:</span> {requestData.DurationName}</p>
					        </div>

                            <div style='background-color:none;width:50%;float: left;'>
							    <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Barcode:</span> {requestData.QRCode}</p>
						    </div>

                            <div style='background-color:none;width:50%;float: left;'>
						        {msg}
						    </div>

					        <div style='background-color:none;width:100%;float: left;'>
						        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Pupose:</span> {requestData.PurposeName}</p>
					        </div>
    
					         <div style='background-color:none;width:100%;float: left;'>
						        <p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Remarks:</span> {requestData.Remark}</p>
					        </div>

					        <div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Name:</span> {requestData.Requester}</p>
						</div>
    
						<div class='box-text2'style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Mobile No:</span> {gatepassData.ContactNumber}</p>
						</div>
    
						<div style='background-color:none;float: left;'>
							<p style='color: #666;padding-top: 0px; font-size: 14px;'><span style='font-weight: 600;color:#333;'>Designation:</span> {gatepassData.DesignationName}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Register No:</span> {gatepassData.RegisterNo}</p>
						</div>
    
						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Vehicle Size:</span> {gatepassData.VehicleSize}</p>
						</div>

						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Plate No:</span> {gatepassData.PlateNo}</p>
						</div>

						<div style='background-color:none;width:50%;float: left;'>
							<p style='color: #666;padding-left:0px;font-size: 14px;'><span style='font-weight: 600;color:#333;'>Type of Vehicle:</span> {gatepassData.VehicleTypeName}</p>
						</div>
                        </div>
                    </body>    
					</html>";
            await _emailSender.SendEmailAsync(requestData.RequesterEmail, "Visit Request Accepted", body, tran);

        }

        public async Task<List<RequesterPostViewModel>> GetPassengersAsync(int requestId, IDbTransaction tran = null)
        {
            return (await _dbContext.GetEnumerableAsync<RequesterPostViewModel>($@"SELECT EmployeeID, EmployeeName, DesignationName, CompanyName, Email, ContactNumber
            FROM Employee E
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
			Where EmployeeID in(Select EmployeeID from RequestPassenger where RequestID=@RequestID and IsDeleted=0)", new { RequestID = requestId })).ToList();
        }
        public async Task<List<RequesterPostViewModel>> GetDailyPassPassengersAsync(int requestId, IDbTransaction tran = null)
        {
            return (await _dbContext.GetEnumerableAsync<RequesterPostViewModel>($@"SELECT EmployeeID, EmployeeName, DesignationName, CompanyName, Email, ContactNumber
            FROM Employee E
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
			Where EmployeeID in(Select EmployeeID from RequestPassenger where DailyPassRequestID=@RequestID and IsDeleted=0)", new { RequestID = requestId })).ToList();
        }

        public async Task<RequesterPostViewModel> GetDriverDetailsAsync(int requestId, IDbTransaction tran = null)
        {
            return await _dbContext.GetAsync<RequesterPostViewModel>($@"SELECT E.EmployeeID, EmployeeName, DesignationName, CompanyName, Email, ContactNumber
            FROM Employee E
			JOIN RequestVehicle R on R.EmployeeID=E.EmployeeID
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
			Where R.RequestID=@RequestID", new { RequestID = requestId });
        }

        public async Task<RequesterPostViewModel> GetDailyPassDriverDetailsAsync(int requestId, IDbTransaction tran = null)
        {
            return await _dbContext.GetAsync<RequesterPostViewModel>($@"SELECT E.EmployeeID, EmployeeName, DesignationName, CompanyName, Email, ContactNumber
            FROM Employee E
			JOIN DailyPassRequest R on R.DriverID=E.EmployeeID
            LEFT JOIN EmployeeDesignation D on D.DesignationID = E.DesignationID
            LEFT JOIN Company C on C.CompanyID = E.CompanyID
			Where R.DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = requestId });
        }

        public async Task<VehicleListViewModel> GetVehicleDetailsAsync(int requestId, IDbTransaction tran = null)
        {
            return await _dbContext.GetAsync<VehicleListViewModel>($@"Select V.VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo 
            From Vehicle V
			JOIN RequestVehicle R on R.VehicleID=V.VehicleID
            LEFT JOIN VehicleType T on V.VehicleTypeID=T.VehicleTypeID
			Where R.RequestID=@RequestID", new { RequestID = requestId });
        }

        public async Task<VehicleListViewModel> GetDailyPassVehicleDetailsAsync(int requestId, IDbTransaction tran = null)
        {
            return await _dbContext.GetAsync<VehicleListViewModel>($@"Select V.VehicleID, RegisterNo, VehicleTypeName, VehicleSize, PlateNo 
            From Vehicle V
			JOIN DailyPassRequest R on R.VehicleID=V.VehicleID
            LEFT JOIN VehicleType T on V.VehicleTypeID=T.VehicleTypeID
			Where R.DailyPassRequestID=@DailyPassRequestID", new { DailyPassRequestID = requestId });
        }

        public async Task<List<MeterialViewModel>> GetMeterialsAsync(int requestId, IDbTransaction tran = null)
        {
            var isPojectAsset = await _dbContext.GetAsync<bool>($@"Select ISNULL(IsProjectAsset,0) from viRequest Where RequestID={requestId}", tran);
            List<MeterialViewModel> meterials = null;
            if (isPojectAsset)
            {
                meterials = (await _dbContext.GetEnumerableAsync<MeterialViewModel>($@"Select *,Name as Description,Quantity,PurchaseUnit as PackingTypeName 
                        From RequestItem R
                        JOIN ItemMaster I on I.ItemID=R.ItemID
                        Where R.IsDeleted=0 and RequestID=@RequestID", new { RequestID = requestId })).ToList();
            }
            else
            {
                meterials = (await _dbContext.GetEnumerableAsync<MeterialViewModel>($@"Select Description,Quantity,P.PackingTypeName 
                        from RequestMeterial M
                        LEFT JOIN PackingType P on M.PackingTypeID=P.PackingTypeID
                        Where M.IsDeleted=0 and RequestID=@RequestID", new
                {
                    RequestID = requestId
                })).ToList();
            }
            return meterials;
        }

        public async Task<List<MeterialViewModel>> GetDailyPassMeterialsAsync(int requestId, IDbTransaction tran = null)
        {
            var meterials = (await _dbContext.GetEnumerableAsync<MeterialViewModel>($@"Select Description,Quantity,P.PackingTypeName 
                        from RequestMeterial M
                        LEFT JOIN PackingType P on M.PackingTypeID=P.PackingTypeID
                        Where M.IsDeleted=0 and DailyPassRequestID=@RequestID", new
            {
                RequestID = requestId
            })).ToList();
            return meterials;
        }

        public async Task<List<RequestApprovalHistoryViewModel>> GetRequestApprovalHistory(int requestId, int timeZoneMinutes)
        {
            return (await _dbContext.GetEnumerableAsync<RequestApprovalHistoryViewModel>($@"Select S.Stage,T.DisplayName as UserGroup,U.Name ApprovedBy,DATEADD(MINUTE,{timeZoneMinutes}, A.AddedOn) as ProcessedOn,Case when A.RequestApprovalID is null then 'Pending' when A.IsRejected=1 then 'Rejected' else 'Accepted' end as Status ,ISNULL(A.Remarks,'') as Remarks
            From RequestTypeApprovalStage S
            JOIN UserTypes T on T.UserTypeID=S.UserTypeID
            JOIN viRequest R on S.RequestTypeID=R.RequestTypeID
            LEFT JOIN RequestApproval A on A.RequestID=R.RequestID and A.ApprovalStage=S.Stage
            LEFT JOIN viPersonalInfos U on U.UserID=A.AddedBy
            Where R.RequestID=@RequestID
            Order by S.Stage", new { RequestID = requestId })).ToList();
        }

        public async Task<List<RequestApprovalHistoryViewModel>> GetDailyPassRequestApprovalHistory(int requestId, int timeZoneMinutes)
        {
            return (await _dbContext.GetEnumerableAsync<RequestApprovalHistoryViewModel>($@"Select S.Stage,T.DisplayName as UserGroup,U.Name ApprovedBy,DATEADD(MINUTE,{timeZoneMinutes}, A.AddedOn) as ProcessedOn,Case when A.RequestApprovalID is null then 'Pending' when A.IsRejected=1 then 'Rejected' else 'Accepted' end as Status ,ISNULL(A.Remarks,'') as Remarks
            From RequestTypeApprovalStage S
            JOIN UserTypes T on T.UserTypeID=S.UserTypeID
            JOIN viDailyPassRequest R on S.RequestTypeID=R.RequestTypeID
            LEFT JOIN RequestApproval A on A.DailyPassRequestID=R.DailyPassRequestID and A.ApprovalStage=S.Stage
            LEFT JOIN viPersonalInfos U on U.UserID=A.AddedBy
            Where R.DailyPassRequestID=@RequestID", new
            {
                RequestID = requestId
            })).ToList();
        }

        public async Task<List<VehicleTrackingHistoryViewModel>> GetVehicleTrackingHistory(int requestId, int timeZoneMinutes)
        {
            return (await _dbContext.GetEnumerableAsync<VehicleTrackingHistoryViewModel>($@"Select P.Name ProcessedBy,case when IsCheckOut=1 then 'Checkout' else 'Checkin' end as Status,DATEADD(MINUTE,{timeZoneMinutes}, V.AddedOn) as AddedOn
            From RequestVehicle V
            JOIN RequestVehicleTracking T on V.RequestVehicleID=T.RequestVehicleID
            JOIN viPersonalInfos P on P.UserID=V.AddedBy
            Where V.RequestID=@RequestID", new { RequestID = requestId })).ToList();
        }


        public async Task<List<VehicleTrackingHistoryViewModel>> GetDailyPassVehicleTrackingHistory(int requestId, int timeZoneMinutes)
        {
            return (await _dbContext.GetEnumerableAsync<VehicleTrackingHistoryViewModel>($@"Select P.Name ProcessedBy,case when IsCheckOut=1 then 'Checkout' else 'Checkin' end as Status,DATEADD(MINUTE,{timeZoneMinutes}, V.AddedOn) as AddedOn
            From DailyPassRequest V
            JOIN RequestVehicleTracking T on V.VehicleID=T.RequestVehicleID
            JOIN viPersonalInfos P on P.UserID=V.AddedBy
            Where V.DailyPassRequestID=@RequestID", new
            {
                RequestID = requestId
            })).ToList();
        }


        public async Task SendNewRequestMailToApprover(int requestId, string url, IDbTransaction tran = null)
        {
            var requestData = await _dbContext.GetAsync<RequestApproverMailModel>($@"Select R.RequestID,R.Slot, R.EmployeeName, R.CompanyName,
                T.RequestTypeName,UT.Email as EmailAddress,UT.UserTypeName
                from viRequest R
                JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
                JOIN RequestTypeApprovalStage S on S.Stage=ISNULL(R.ApprovalStage,0)+1 and S.RequestTypeID=R.RequestTypeID and (R.NeedHigherLevelApproval=1 or R.StatusID=1) and R.IsRejected=0
                JOIN UserTypes UT on UT.UserTypeID=S.UserTypeID
                JOIN Employee E on E.EmployeeID=R.EmployeeID
                Where R.RequestID=@RequestID", new { RequestID = requestId }, tran);

            if (requestData != null && !string.IsNullOrEmpty(requestData.EmailAddress))
            {
                var body = $@"Hi,<br/>
                    You have got new {requestData.RequestTypeName} request from one of your client {requestData.EmployeeName} at {requestData.CompanyName}.<br>
                    <a href='{url}/approve-request/{requestData.RequestID}'>Click here to approve the request</a>";

                await _emailSender.SendHtmlEmailAsync(requestData.EmailAddress, $"New {requestData.RequestTypeName} Request From {requestData.EmployeeName}", body, tran);
            }
        }

        public async Task SendNewDailyPassRequestMailToApprover(int requestId, string url, IDbTransaction tran = null)
        {
            var requestData = await _dbContext.GetAsync<RequestApproverMailModel>($@"Select R.DailyPassRequestID as RequestID, R.EmployeeName, R.CompanyName,
                T.RequestTypeName,UT.Email as EmailAddress,UT.UserTypeName
                from viDailyPassRequest R
                JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
                JOIN RequestTypeApprovalStage S on S.Stage=ISNULL(R.ApprovalStage,0)+1 and S.RequestTypeID=R.RequestTypeID and (R.NeedHigherLevelApproval=1 or R.StatusID=1) and R.IsRejected=0
                JOIN UserTypes UT on UT.UserTypeID=S.UserTypeID
                JOIN Employee E on E.EmployeeID=R.EmployeeID
                Where R.DailyPassRequestID=@RequestID", new { RequestID = requestId }, tran);

            if (requestData != null && !string.IsNullOrEmpty(requestData.EmailAddress))
            {
                var body = $@"Hi,<br/>
                    You have got new {requestData.RequestTypeName} request from one of your client {requestData.EmployeeName} at {requestData.CompanyName}.<br>
                    <a href='{url}/approve-daily-pass-request/{requestData.RequestID}'>Click here to approve the request</a>";

                await _emailSender.SendHtmlEmailAsync(requestData.EmailAddress, $"New {requestData.RequestTypeName} Request From {requestData.EmployeeName}", body, tran);
            }
        }

        public async Task<MyProfileInfoModel> GetProfileInfo(int CurrentUserID)
        {
            var employeeId = await _dbContext.GetAsync<int>($@"Select Top(1) EmployeeID From Employee Where AddedBy=@AddedBy and IsDeleted=@IsDeleted", new { AddedBy = CurrentUserID, IsDeleted = false });


            var res = await _dbContext.GetAsync<MyProfileInfoModel>($@"Select  ISNULL(P.Name,UserName) as Name, Isnull(ProfileImageFileName,'') as ProfileImage,
            ISNULL(EmailAddress,'') as EmailAddress,ISNULL(MobileNumber,'') as MobileNumber, T.DisplayName as UserTypeName, QRCode,C.CompanyName
            from Users U
			LEFT JOIN viPersonalInfos P on U.PersonalInfoID=P.PersonalInfoID
            LEFT JOIN UserTypes T on T.UserTypeID=U.UserTypeID
			LEFT JOIN Employee E on E.EmployeeID=@EmployeeID
            LEFT JOIN Company C on C.CompanyID=E.CompanyID
			where U.UserID=@UserID", new { EmployeeID = employeeId, UserID = CurrentUserID });

            return res;
        }

        public async Task<IEnumerable<IdnValuePair>> GetCopassengers(int CurrentUserID, IDbTransaction tran = null)
        {
            var AddedCoPassengers = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT  EmployeeID as ID, EmployeeName as Value
                FROM Employee
                Where IsDeleted = 0 and AddedBy = {CurrentUserID}", null, tran)).ToList();
            if (AddedCoPassengers.Count > 0)
                AddedCoPassengers.RemoveAt(0);

            return AddedCoPassengers;
        }

        

    }
}
