using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository
{
    public interface IErrorLogRepository
    {
        Task<BaseResponse> CreatThrowResponse(string description, int? userId);
        void Log(string message);

    }
    public class ErrorLogRepository: IErrorLogRepository
    {
        private readonly IDbContext _dbContext;

        public ErrorLogRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Log(string message)
        {
            // Example: write to file or database
            System.IO.File.AppendAllText("Logs/ErrorLog.txt", DateTime.Now + " - " + message + Environment.NewLine);
        }

        public async Task<BaseResponse> CreatThrowResponse(string description, int? userId)
        {
            ErrorLog log = new ErrorLog()
            {
                AddedOn = DateTime.UtcNow,
                UserID = userId,
                Error = description,
            };
            log.ErrorLogID = await _dbContext.SaveAsync(log);

            BaseResponse res = new BaseResponse()
            {
                ResponseCode = -1,
                ResponseTitle = "Error ID:"+ log.ErrorLogID.ToString(),
                ResponseMessage = "Something went wrong! Please Contact Support Center",
            };
            return res;
        }
    }
}
