using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Louvre.Shared.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Louvre.Helpers
{
    public class Middleware
    {
        //Reference:https://jasonwatmore.com/post/2020/10/02/aspnet-core-31-global-error-handler-tutorial

        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public Middleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.GetTypedHeaders().AcceptLanguage.FirstOrDefault() != null)
            {
                var lang = context.Request.GetTypedHeaders().AcceptLanguage.FirstOrDefault().ToString();
                var cultureInfo = new CultureInfo(lang);
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            }

            try
            {
                //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'");
                await _next(context);
            }
            catch (PreDefinedException error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                string result = "";
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new BaseResponse()
                {
                    ResponseCode = -1000,
                    ResponseMessage = error.Response.ResponseMessage,
                    ResponseTitle = error.Response.ResponseTitle,
                    //ResponseErrorDescription = error.Response.ResponseErrorDescription 
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                await response.WriteAsync(result);
            }
            catch (Exception error)
            {
                var response = context.Response;
                BaseResponse errorObj = new BaseResponse() { ResponseTitle = "Oops something went wrong",ResponseMessage= "Please contact support center" };
                var configValue = _config["NeedErrorLog"];
                if (!string.Equals(configValue, "false", StringComparison.OrdinalIgnoreCase))
                {
                    response.ContentType = "application/json";
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    string randomErrorCode = new string(Enumerable.Repeat(chars, 6)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    errorObj = new BaseResponse() { ResponseCode=-1000, ResponseMessage = string.Concat("Error Code: ", randomErrorCode, "\n Please contact support center"), ResponseTitle = "Oops something went wrong" };

                    randomErrorCode = string.Concat("\n", randomErrorCode, " generated On ", DateTime.UtcNow.AddMinutes(330).ToString(), "\n");
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"ErrorLog{DateTime.UtcNow.AddMinutes(330).ToString("dd-MM-yyyy")}.txt");
                    string errorLog = string.Concat(randomErrorCode, error.ToString());
                    using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(errorLog);
                        }
                    }
                }
                string result = JsonSerializer.Serialize(errorObj, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(result);
            }
        }
    }
}
