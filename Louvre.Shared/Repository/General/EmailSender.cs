using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository
{
    public interface IEmailSender
    {
        Task<BaseResponse> SendEmailAsync(string email, string subject, string message, IDbTransaction tran = null);
        Task<BaseResponse> SendHtmlEmailAsync(string email, string subject, string message, IDbTransaction tran = null);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IDbContext _dbContext;
        public EmailSender(IDbContext entity)
        {
            _dbContext = entity;
        }

        public async Task<BaseResponse> SendEmailAsync(string email, string subject, string message, IDbTransaction tran = null)
        {
            var result = await Send(email, subject, message, tran);
            return result;
        }

        public async Task<BaseResponse> SendHtmlEmailAsync(string email, string subject, string message, IDbTransaction tran = null)
        {
            var body = $@"<!DOCTYPE html>
					    <html>
					    <head>
					    <title>{subject}</title>
					    <link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
					    </head>
					    <body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>
						   {message}
					    </body>    
					    </html>";

            var result = await Send(email, subject, body, tran);
            return result;
        }

        public async Task<BaseResponse> Send(string email, string subject, string message, IDbTransaction tran = null)
        {
            BaseResponse response = new BaseResponse();
            try
            {

                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var emailSettings = await _dbContext.GetAsync<MailSettings>(1, tran);

                string toEmail = string.IsNullOrEmpty(email) ? emailSettings.MailTo : email;

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(emailSettings.FromMail, emailSettings.FromName)
                };

                foreach (var address in email.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                //For inline image 
                AlternateView alterView = ContentToAlternateView(message);
                mail.AlternateViews.Add(alterView);


                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = emailSettings.SMTPHost;
                    smtpClient.Port = emailSettings.Port; // Google smtp port
                    smtpClient.EnableSsl = emailSettings.EnableSSL;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(emailSettings.FromMail, emailSettings.Password);
                    await smtpClient.SendMailAsync(mail);
                    response.CreatSuccessResponse();
                }

                await _dbContext.SaveAsync(new SentMail()
                {
                    EmailAddress = email,
                    Subject = subject,
                    HasSent = true,
                    Message = message,
                    SentOn = DateTime.UtcNow,
                }, tran);

                //using (SmtpClient smtp = new SmtpClient(emailSettings.SMTPHost, emailSettings.Port))
                //{
                //	//smtp.UseDefaultCredentials = true;
                //	smtp.Credentials = new NetworkCredential(emailSettings.FromMail, emailSettings.Password);
                //	smtp.EnableSsl = true;

                //	await smtp.SendMailAsync(mail);
                //	response.CreatSuccessResponse();
                //}
            }
            catch (Exception ex)
            {
                await _dbContext.SaveAsync(new SentMail()
                {
                    EmailAddress = email,
                    Subject = subject,
                    HasSent = false,
                    Message = message,
                    SentOn = DateTime.UtcNow,
                    ErrorLog = ex.Message
                }, tran);

                response.CreatErrorResponse(-4, ex.Message);
            }
            return response;
        }

        private static AlternateView ContentToAlternateView(string content)
        {
            var imgCount = 0;
            List<LinkedResource> resourceCollection = new List<LinkedResource>();
            foreach (Match m in Regex.Matches(content, "<img(?<value>.*?)>"))
            {
                imgCount++;
                var imgContent = m.Groups["value"].Value;
                string type = Regex.Match(imgContent, ":(?<type>.*?);base64,").Groups["type"].Value;
                string base64 = Regex.Match(imgContent, "base64,(?<base64>.*?)\"").Groups["base64"].Value;

                if (String.IsNullOrEmpty(type) || String.IsNullOrEmpty(base64))
                {
                    //ignore replacement when match normal <img> tag
                    continue;
                }
                var width = Regex.Match(imgContent, "width='(\\d+)'").Groups[1].Value;
                var replacement = " src=\"cid:" + imgCount + "\" width=\"" + width + "\"";
                content = content.Replace(imgContent, replacement);
                var tempResource = new LinkedResource(Base64ToImageStream(base64), new ContentType(type))
                {
                    ContentId = imgCount.ToString()
                };
                resourceCollection.Add(tempResource);
            }

            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(content, null, MediaTypeNames.Text.Html);
            foreach (var item in resourceCollection)
            {
                alternateView.LinkedResources.Add(item);
            }

            return alternateView;
        }

        public static Stream Base64ToImageStream(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            return ms;
        }
    }
}
