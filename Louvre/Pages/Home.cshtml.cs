using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize]
    [BindProperties]
    public class HomeModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeModel(IDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }


        public int RequestStatusID { get; set; }

        public int CheckinStatus { get; set; }

        public List<int> MeterialRequestAccepted { get; set; }
        public List<int> MeterialRequestRequested { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var clientTime = GetClientTime(_httpContextAccessor)!.Date;

            #region Calendar

            if (User.IsInRole("Super-Admin") || User.IsInRole("Administrator") || User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                ViewData["CalenderData"] = (await _dbContext.GetEnumerableAsync<CalenderViewModel>($@"SELECT  Convert(varchar,Date)+'T'+S.TimeFrom as Start,Convert(varchar,Date)+'T'+S.TimeTo as [End],S.TimeFrom+'-'+ S.TimeTo as Title,S.RequestCount,ISNULL(RequestedCount,0) as RequestedCount, S.SlotID as id
                FROM SlotMaster M
                JOIN Slot S on S.SlotMasterID=M.SlotMasterID and S.IsDeleted=0
                LEFT JOIN (SELECT   SlotID,Count(SlotID) as RequestedCount
                FROM viRequest
                Where StatusID={(int)RequestStatus.Accepted}
                Group by SlotID) R on R.SlotID=S.SlotID
                Where M.Date>=@Date and M.IsDeleted=0", new { Date = clientTime })).ToList();

                ViewData["CalenderDetails"] = (await _dbContext.GetEnumerableAsync<CalenderDetails>($@"SELECT SlotID, EmployeeName, ModeName, LocationName,StatusID, Slot
                FROM viRequest
                Where Date>=@Date and StatusID={(int)RequestStatus.Accepted}", new { Date = clientTime })).ToList();
            }

            #endregion

            #region Graph _MonthSummaryChart

            var slotBefore = Convert.ToInt32(await _dbContext.GetAsync<int>("Select SettingsValue From GeneralSettings Where SettingsKey=@SettingsKey", new { SettingsKey = "SlotSelectionBefore" }));
            var clientDate = GetClientTime(_httpContextAccessor);
            var fromDate = clientDate.AddDays(-1 * (31 - slotBefore));
            var toDate = clientDate.AddDays(slotBefore);

            int notArrived = 0;
            int vehicleIn = 0;
            int vehicleOut = 0;
            int dnotArrived = 0;
            int dvehicleIn = 0;
            int dvehicleOut = 0;

            if (User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                var sql = @"
                    SELECT ISNULL(StatusID,0) as Value 
                    FROM [dbo].[GetDates](@FromDate, @ToDate) D
                    LEFT JOIN (
                        SELECT R.Date, COUNT(StatusID) as StatusID 
                        FROM viRequest R 
                        JOIN RequestTypeApprovalStage S on S.RequestTypeID = R.RequestTypeID 
                        JOIN UserTypes U on S.UserTypeID = U.UserTypeID
                        WHERE U.UserTypeID = @UserTypeID
                            AND StatusID >= 4
                            AND R.Date BETWEEN @FromDate AND @ToDate
                            AND S.Stage <= CASE 
                                WHEN U.UserNature = 2 AND R.IsDisposalRequired = 1 THEN ISNULL(R.ApprovalStage, 0) + 2 
                                ELSE ISNULL(R.ApprovalStage, 0) + 1 
                            END
                        GROUP BY R.Date
                    ) R ON R.Date = D.Date
                    ORDER BY D.Date
                ";

                ViewData["MeterialRequestAccepted"] = (await _dbContext.GetEnumerableAsync<int>(
                    sql,
                    new
                    {
                        UserTypeID = CurrentUserTypeID,
                        FromDate = fromDate,
                        ToDate = toDate
                    }
                )).ToList();


                ViewData["MeterialRequestRequested"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate, @ToDate) D
                    LEFT JOIN(Select R.Date,Count(StatusID) as StatusID 
                    From viRequest R 
                    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                    JOIN UserTypes U on  S.UserTypeID=U.UserTypeID and U.UserTypeID=@UserTypeID and R.Date between @FromDate and @ToDate
                    and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                    Group by R.Date)R on R.Date=D.Date
                    order by D.Date", new { UserTypeID = CurrentUserTypeID, FromDate = fromDate, ToDate = toDate })).ToList();

                ViewData["Dates"] = (await _dbContext.GetEnumerableAsync<string>($@"SELECT  Convert(varchar,Date)+'T00:00:00.000Z' Date
                    FROM  [dbo].[GetDates](@FromDate,@ToDate)", new { FromDate = fromDate, ToDate = toDate })).ToList();


                ViewData["DailyPassRequestAccepted"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate,@ToDate) D
                    LEFT JOIN(
                        Select D.Date,Count(StatusID) as StatusID
                        From viDailyPassRequest R 
                        JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                        JOIN UserTypes U on  S.UserTypeID=U.UserTypeID 
					    JOIN [dbo].[GetDates](@FromDate,@ToDate) D on D.Date <=R.ToDate
                        LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where CONVERT(varchar,AddedOn,103)=@ClientDate Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                        LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                        Where U.UserTypeID=@UserTypeID and StatusID>=4
                        and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                        Group By D.Date)R on R.Date=D.Date
                    order by D.Date", new { UserTypeID = CurrentUserTypeID, FromDate = fromDate, ToDate = toDate, ClientDate = clientDate })).ToList();

                ViewData["DailyPassRequestRequested"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate,@ToDate) D
                    LEFT JOIN(
                        Select D.Date,Count(StatusID) as StatusID
                        From viDailyPassRequest R 
                        JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                        JOIN UserTypes U on  S.UserTypeID=U.UserTypeID 
					    JOIN [dbo].[GetDates](@FromDate,@ToDate) D on D.Date  <=R.ToDate
                        LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where CONVERT(varchar,AddedOn,103)=@ClientDate Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                        LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                        Where U.UserTypeID=@UserTypeID
                        and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                        Group By D.Date)R on R.Date=D.Date
                    order by D.Date", new { UserTypeID = CurrentUserTypeID, FromDate = fromDate, ToDate = toDate, ClientDate = clientDate })).ToList();



                var checkinSummary = await _dbContext.GetEnumerableAsync<CheckoutSummaryModel>($@"Select IsCheckOut,Count(*) Cnt
                    From viRequest R 
                    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                    JOIN UserTypes U on  S.UserTypeID=U.UserTypeID 
                    JOIN RequestVehicle D on D.RequestID=R.RequestID
                    LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                    LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
                    Where U.UserTypeID=@UserTypeID and StatusID>=4 and R.Date=@ClientDate
                    and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                    Group By IsCheckOut", new { UserTypeID = CurrentUserTypeID, ClientDate = clientDate });

                if (checkinSummary != null)
                {
                    notArrived = checkinSummary.Where(s => s.IsCheckOut == null).Select(s => s.Cnt).FirstOrDefault();
                    vehicleIn = checkinSummary.Where(s => s.IsCheckOut == false).Select(s => s.Cnt).FirstOrDefault();
                    vehicleOut = checkinSummary.Where(s => s.IsCheckOut == true).Select(s => s.Cnt).FirstOrDefault();
                }


                var dailyCheckinSummary = await _dbContext.GetEnumerableAsync<CheckoutSummaryModel>($@"Select IsCheckOut,Count(*) Cnt
                    From viDailyPassRequest R 
                    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                    JOIN UserTypes U on  S.UserTypeID=U.UserTypeID 
                    LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where CONVERT(varchar,AddedOn,103)='{clientDate}' Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                    LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                    Where U.UserTypeID={CurrentUserTypeID} and StatusID>=4 and R.FromDate <=@ClientDate and R.ToDate>=@ClientDate
                    and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                    Group By IsCheckOut", new { UserTypeID = CurrentUserTypeID, ClientDate = clientDate });

                if (dailyCheckinSummary != null)
                {
                    dnotArrived = dailyCheckinSummary.Where(s => s.IsCheckOut == null).Select(s => s.Cnt).FirstOrDefault();
                    dvehicleIn = dailyCheckinSummary.Where(s => s.IsCheckOut == false).Select(s => s.Cnt).FirstOrDefault();
                    dvehicleOut = dailyCheckinSummary.Where(s => s.IsCheckOut == true).Select(s => s.Cnt).FirstOrDefault();
                }
            }
            else if (User.IsInRole("Monitor") || User.IsInRole("Super-Admin") || User.IsInRole("Administrator"))
            {
                ViewData["MeterialRequestAccepted"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate,@ToDate) D
                    LEFT JOIN(Select R.Date,Count(StatusID) as StatusID 
                    From viRequest R 
					Where StatusID>=4 and R.Date between @FromDate and @ToDate
                    Group by R.Date)R on R.Date=D.Date
                    order by D.Date", new { UserTypeID = CurrentUserTypeID, FromDate = fromDate, ToDate = toDate, ClientDate = clientDate })).ToList();

                ViewData["MeterialRequestRequested"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate,@ToDate) D
                    LEFT JOIN(Select R.Date,Count(StatusID) as StatusID 
                    From viRequest R 
					Where R.Date between @FromDate and @ToDate
                    Group by R.Date)R on R.Date=D.Date
                    order by D.Date", new { UserTypeID = CurrentUserTypeID, FromDate = fromDate, ToDate = toDate, ClientDate = clientDate })).ToList();

                ViewData["Dates"] = (await _dbContext.GetEnumerableAsync<string>($@"SELECT  Convert(varchar,Date)+'T00:00:00.000Z' Date
                    FROM  [dbo].[GetDates](@FromDate,@ToDate)", new { FromDate = fromDate, ToDate = toDate })).ToList();


                ViewData["DailyPassRequestAccepted"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate,@ToDate) D
                    LEFT JOIN(
                        Select D.Date,Count(StatusID) as StatusID
                        From viDailyPassRequest R 
					    JOIN [dbo].[GetDates](@FromDate,@ToDate) D on D.Date between R.FromDate and R.ToDate
                        Where StatusID>=4 
                        Group By D.Date)R on R.Date=D.Date
                    order by D.Date", new { FromDate = fromDate, ToDate = toDate })).ToList();

                ViewData["DailyPassRequestRequested"] = (await _dbContext.GetEnumerableAsync<int>($@"SELECT ISNULL(StatusID,0) as Value 
                    FROM  [dbo].[GetDates](@FromDate,@ToDate) D
                    LEFT JOIN(
                        Select D.Date,Count(StatusID) as StatusID
                        From viDailyPassRequest R 
					    JOIN [dbo].[GetDates](@FromDate,@ToDate) D on D.Date between R.FromDate and R.ToDate
                        Group By D.Date)R on R.Date=D.Date
                    order by D.Date", new { FromDate = fromDate, ToDate = toDate })).ToList();



                var checkinSummary = await _dbContext.GetEnumerableAsync<CheckoutSummaryModel>($@"Select IsCheckOut,Count(*) Cnt
                    From viRequest R  
                    JOIN RequestVehicle D on D.RequestID=R.RequestID
                    LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                    LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
                    Where StatusID>=4 and R.Date=@Date
                    Group By IsCheckOut", new { Date = clientDate });

                if (checkinSummary != null)
                {
                    notArrived = checkinSummary.Where(s => s.IsCheckOut == null).Select(s => s.Cnt).FirstOrDefault();
                    vehicleIn = checkinSummary.Where(s => s.IsCheckOut == false).Select(s => s.Cnt).FirstOrDefault();
                    vehicleOut = checkinSummary.Where(s => s.IsCheckOut == true).Select(s => s.Cnt).FirstOrDefault();
                }


                var dailyCheckinSummary = await _dbContext.GetEnumerableAsync<CheckoutSummaryModel>($@"Select IsCheckOut,Count(*) Cnt
                    From viDailyPassRequest R 
                    LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where CONVERT(varchar,AddedOn,103)='{clientDate}' Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                    LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                    Where StatusID>=4 and R.FromDate <=@Date and R.ToDate>=@Date
                    Group By IsCheckOut", new { Date = clientDate });

                if (dailyCheckinSummary != null)
                {
                    dnotArrived = dailyCheckinSummary.Where(s => s.IsCheckOut == null).Select(s => s.Cnt).FirstOrDefault();
                    dvehicleIn = dailyCheckinSummary.Where(s => s.IsCheckOut == false).Select(s => s.Cnt).FirstOrDefault();
                    dvehicleOut = dailyCheckinSummary.Where(s => s.IsCheckOut == true).Select(s => s.Cnt).FirstOrDefault();
                }
            }

            #endregion


            ViewData["NotArrived"] = notArrived;
            ViewData["In"] = vehicleIn;
            ViewData["Out"] = vehicleOut;
            ViewData["TodaysTotal"] = notArrived + vehicleIn + vehicleOut;


            ViewData["DNotArrived"] = dnotArrived;
            ViewData["DIn"] = dvehicleIn;
            ViewData["DOut"] = dvehicleOut;
            ViewData["DTodaysTotal"] = dnotArrived + dvehicleIn + dvehicleOut;

            var summary = new List<IdnValuePair>();
            var dailyPassSummary = new List<IdnValuePair>();

            if (User.IsInRole("Company") || User.IsInRole("Individual"))
            {
                var requestCount = await _dbContext.ExecuteScalarAsync<int>($@"SELECT COUNT(RequestID) FROM viRequest WHERE (RequestedByID ={CurrentUserID}) and Date>=@Date", new { Date = clientDate });
                ViewData["TotalRequests"] = requestCount;

                var dailyrequestCount = await _dbContext.ExecuteScalarAsync<int>($@"SELECT COUNT(DailyPassRequestID) FROM viDailyPassRequest WHERE (RequestedByID ={CurrentUserID}) and ToDate>=@Date", new { Date = clientDate });
                ViewData["TotalDailyRequests"] = dailyrequestCount;

                summary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT StatusID as ID,Count(StatusID) as Value FROM viRequest WHERE (RequestedByID = {CurrentUserID}) and Date>=@Date Group by StatusID", new { Date = clientDate })).ToList();
                dailyPassSummary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT StatusID as ID,Count(StatusID) as Value FROM viDailyPassRequest WHERE (RequestedByID = {CurrentUserID}) and ToDate>=@Date Group by StatusID", new { Date = clientDate })).ToList();
            }
            else if (User.IsInRole("Monitor") || User.IsInRole("Super-Admin") || User.IsInRole("Administrator"))
            {
                int totalRequest = 0;
                summary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT  StatusID as ID,Count(StatusID) as Value 
                    FROM  viRequest R
                    Where Date>=@Date
                    Group by StatusID", new { Date = clientDate })).ToList();

                foreach (var item in summary)
                {
                    totalRequest += Convert.ToInt32(item.Value);
                }
                ViewData["TotalRequests"] = totalRequest;


                dailyPassSummary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT  StatusID as ID,Count(StatusID) as Value 
                    FROM  viDailyPassRequest R
                    Where ToDate>=@Date
                    Group by StatusID", new
                {
                    Date = clientDate
                })).ToList();
                ViewData["TotalDailyRequests"] = dailyPassSummary.Sum(s => Convert.ToInt32(s.Value));
            }
            else
            {
                int totalRequest = 0;
                summary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT  StatusID as ID,Count(StatusID) as Value 
                    FROM  viRequest R
                    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                    JOIN UserTypes U on  S.UserTypeID=U.UserTypeID
                    and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                    Where Date>=@Date and U.UserTypeID={CurrentUserTypeID} 
                    Group by StatusID", new { Date = clientDate })).ToList();

                foreach (var item in summary)
                {
                    totalRequest += Convert.ToInt32(item.Value);
                }
                ViewData["TotalRequests"] = totalRequest;

                dailyPassSummary = (await _dbContext.GetEnumerableAsync<IdnValuePair>($@"SELECT  StatusID as ID,Count(StatusID) as Value 
                    FROM  viDailyPassRequest R
                    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                    JOIN UserTypes U on  S.UserTypeID=U.UserTypeID
                    and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                    Where ToDate>=@Date and U.UserTypeID={CurrentUserTypeID} 
                    Group by StatusID", new { Date = clientDate })).ToList();
                ViewData["TotalDailyRequests"] = dailyPassSummary.Sum(s => Convert.ToInt32(s.Value));
            }

            ViewData["PendingRequests"] = summary.Where(s => s.ID == (int)RequestStatus.Pending).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["AcceptedRequests"] = summary.Where(s => s.ID == (int)RequestStatus.Accepted).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["RejectedRequests"] = summary.Where(s => s.ID == (int)RequestStatus.Rejected).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["ProcessingRequests"] = summary.Where(s => s.ID == (int)RequestStatus.Processing).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["CompletedRequests"] = summary.Where(s => s.ID == (int)RequestStatus.Completed).Select(s => s.Value).FirstOrDefault() ?? "0";

            ViewData["PendingDailyPassRequests"] = dailyPassSummary.Where(s => s.ID == (int)RequestStatus.Pending).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["AcceptedDailyPassRequests"] = dailyPassSummary.Where(s => s.ID == (int)RequestStatus.Accepted).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["ProcessingDailyPassRequests"] = dailyPassSummary.Where(s => s.ID == (int)RequestStatus.Processing).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["RejectedDailyPassRequests"] = dailyPassSummary.Where(s => s.ID == (int)RequestStatus.Rejected).Select(s => s.Value).FirstOrDefault() ?? "0";
            ViewData["CompletedDailyPassRequests"] = dailyPassSummary.Where(s => s.ID == (int)RequestStatus.Completed).Select(s => s.Value).FirstOrDefault() ?? "0";

            return Page();
        }

        public async Task<IActionResult> OnPostGetRequestsDetailAsync()
        {
            var clientTime = GetClientTime(_httpContextAccessor);
            string query = "";
            if (User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                if (CurrentUserTypeID == 3)
                {
                    query = $@"SELECT DISTINCT RequestID, RequestNo, Convert(varchar, Date,103) Date, EmployeeName, BranchName, 
                SubBranchName, RequestedSlot, ModeName, Slot, LocationName, StatusID, T.RequestTypeName,Remarks,CompanyName,R.RequestTypeID,EmployeeID
                FROM  viRequest R
                LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
			    LEFT JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
			    LEFT JOIN UserTypes U on  S.UserTypeID=U.UserTypeID and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                Where Date>=@Date and U.UserTypeID={CurrentUserTypeID}";
                }
                else
                {
                    query = $@"SELECT DISTINCT RequestID, RequestNo, Convert(varchar, Date,103) Date, EmployeeName, BranchName, 
                SubBranchName, RequestedSlot, ModeName, Slot, LocationName, StatusID, T.RequestTypeName,Remarks,CompanyName,R.RequestTypeID
                FROM  viRequest R
                LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
			    LEFT JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
			    LEFT JOIN UserTypes U on  S.UserTypeID=U.UserTypeID and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                Where Date>=@Date and U.UserTypeID={CurrentUserTypeID}";
                }

            }
            else //if(User.IsInRole("Monitor") || User.IsInRole("Super-Admin") || User.IsInRole("Administrator"))
            {
                query = $@"SELECT DISTINCT RequestID, RequestNo, Convert(varchar, Date,103) Date, EmployeeName, BranchName, 
                SubBranchName, RequestedSlot, ModeName, Slot, LocationName, StatusID, T.RequestTypeName,Remarks,CompanyName,R.RequestTypeID
                FROM  viRequest R
                LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
			    Where Date>=@Date";
            }

            if (RequestStatusID != 0)
            {
                query += $" and StatusID ={RequestStatusID}";
            }

            if (User.IsInRole("Company") || User.IsInRole("Individual"))
            {
                query += $" and (RequestedByID ={CurrentUserID})";
            }

            var result = await _dbContext.GetEnumerableAsync<RequestListPopupViewModel>(query, new { Date = clientTime });

            if (CurrentUserTypeID == 3)
            {
                string query1 = @"SELECT EmployeeID, QRCode FROM Employee WHERE EmployeeID IN @EmpIds";
                var empIds = result.Select(x => x.EmployeeID).ToList();

                if (empIds.Any()) // Avoid querying if there are no employee IDs
                {
                    var qrCodes = await _dbContext.GetEnumerableAsync<EmployeeQRCodeModel>(query1, new { EmpIds = empIds });

                    // Merging QR codes into result
                    result = result.Select(r =>
                    {
                        var qrCode = qrCodes.FirstOrDefault(q => q.EmployeeID == r.EmployeeID);
                        r.QRCode = qrCode?.QRCode; // Ensure RequestListPopupViewModel has a QRCode property
                        return r;
                    }).ToList();
                }
            }

            // Use `result` after modification


            return new JsonResult(result);
        }


        public async Task<IActionResult> OnPostGetDailyPassRequestsDetailAsync()
        {
            var clientTime = GetClientTime(_httpContextAccessor);
            string query = "";
            if (User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                query = $@"SELECT  DailyPassRequestID as RequestID, RequestNo, Convert(varchar, FromDate,103) Date,Convert(varchar, ToDate,103) ToDate, EmployeeName, BranchName, 
                    SubBranchName, ModeName, LocationName, StatusID, Remarks,CompanyName,R.RequestTypeID, T.RequestTypeName
                    FROM  viDailyPassRequest R
                    LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
                    JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                    JOIN UserTypes U on  S.UserTypeID=U.UserTypeID
                    and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                    Where ToDate>=@Date and U.UserTypeID={CurrentUserTypeID}";
            }
            else //if(User.IsInRole("Monitor") || User.IsInRole("Super-Admin") || User.IsInRole("Administrator"))
            {
                query = $@"SELECT DailyPassRequestID as RequestID, RequestNo, Convert(varchar, FromDate,103) Date,Convert(varchar, ToDate,103) ToDate, EmployeeName, BranchName, 
                SubBranchName, ModeName, LocationName, StatusID, Remarks,CompanyName,R.RequestTypeID, T.RequestTypeName
                FROM  viDailyPassRequest R
                LEFT JOIN RequestType T on R.RequestTypeID=T.RequestTypeID
			    Where ToDate>=@Date";
            }

            if (RequestStatusID != 0)
            {
                query += $" and StatusID ={RequestStatusID}";
            }

            if (User.IsInRole("Company") || User.IsInRole("Individual"))
            {
                query += $" and (RequestedByID ={CurrentUserID})";
            }

            var result = await _dbContext.GetEnumerableAsync<RequestListPopupViewModel>(query, new { Date = clientTime });

            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostGetCheckinDetailsAsync()
        {
            var clientTime = GetClientTime(_httpContextAccessor);

            string where = "";
            switch (CheckinStatus)
            {
                case 1://not arrived
                    where += $" and IsCheckOut is null";
                    break;
                case 2://Checked in
                    where += $" and IsCheckOut=0";
                    break;
                case 3://Checked out
                    where += $" and IsCheckOut=1";
                    break;
            }

            string query = "";

            if (User.IsInRole("Approver") || User.IsInRole("Disposal"))
            {
                query = $@"Select 1 as Type,R.RequestID, RequestNo, Convert(varchar, Date,103) Date, EmployeeName, BranchName, 
                SubBranchName, ModeName, LocationName, StatusID,Remarks,CompanyName,R.RequestTypeID, RT.RequestTypeName
                From viRequest R  
                JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID 
                JOIN UserTypes U on  S.UserTypeID=U.UserTypeID
                JOIN RequestVehicle D on D.RequestID=R.RequestID
                LEFT JOIN RequestType RT on R.RequestTypeID=RT.RequestTypeID
                LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
                Where StatusID>=4 and R.Date=@Date {where}
                and U.UserTypeID={CurrentUserTypeID} and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end
                
                UNION

                Select 2 as Type,R.DailyPassRequestID as RequestID, RequestNo, Convert(varchar, FromDate,103)+'-'+Convert(varchar, ToDate,103) Date, EmployeeName, BranchName, 
                SubBranchName, ModeName, LocationName, StatusID, Remarks,CompanyName,R.RequestTypeID, RT.RequestTypeName
                From viDailyPassRequest R 
                JOIN RequestTypeApprovalStage S on S.RequestTypeID=R.RequestTypeID
                JOIN UserTypes U on  S.UserTypeID=U.UserTypeID
                LEFT JOIN RequestType RT on R.RequestTypeID=RT.RequestTypeID
                LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where CONVERT(varchar,AddedOn,103)=@Date Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                Where StatusID>=4 and R.FromDate <=@Date and R.ToDate>=@Date {where}
                and U.UserTypeID={CurrentUserTypeID} and S.Stage<= Case when U.UserNature=2 and R.IsDisposalRequired=1 then ISNULL(R.ApprovalStage,0)+2 else ISNULL(R.ApprovalStage,0)+1 end";

            }
            else
            {
                query = $@"Select 1 as Type,R.RequestID, RequestNo, Convert(varchar, Date,103) Date, EmployeeName, BranchName, 
                SubBranchName, ModeName, LocationName, StatusID,Remarks,CompanyName,R.RequestTypeID, RT.RequestTypeName
                From viRequest R  
                JOIN RequestVehicle D on D.RequestID=R.RequestID
                LEFT JOIN RequestType RT on R.RequestTypeID=RT.RequestTypeID
                LEFT JOIN (Select RequestVehicleID,Max(RequestVehicleTrackingID) as RequestVehicleTrackingID From RequestVehicleTracking Group by RequestVehicleID) as TR on TR.RequestVehicleID=D.RequestVehicleID
                LEFT JOIN RequestVehicleTracking T on T.RequestVehicleTrackingID=TR.RequestVehicleTrackingID
                Where StatusID>=4 and R.Date=@Date {where}

                UNION

                Select 2 as Type,R.DailyPassRequestID as RequestID, RequestNo, Convert(varchar, FromDate,103)+'-'+Convert(varchar, ToDate,103) Date, EmployeeName, BranchName, 
                SubBranchName, ModeName, LocationName, StatusID, Remarks,CompanyName,R.RequestTypeID, RT.RequestTypeName
                From viDailyPassRequest R 		
                LEFT JOIN RequestType RT on R.RequestTypeID=RT.RequestTypeID
                LEFT JOIN (Select DailyPassRequestID,Max(TrackingID) as TrackingID From DailyPassRequestTracking Where CONVERT(varchar,AddedOn,103)=@Date Group by DailyPassRequestID) as TR on TR.DailyPassRequestID=R.DailyPassRequestID
                LEFT JOIN DailyPassRequestTracking T on T.TrackingID=TR.TrackingID
                Where StatusID>=4 and R.FromDate <=@Date and R.ToDate>=@Date {where}";
            }
            var result = await _dbContext.GetEnumerableAsync<CheckinRequestListPopupViewModel>(query, new { Date = clientTime });

            return new JsonResult(result);
        }
    }
}