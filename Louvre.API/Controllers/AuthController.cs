
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Progbiz.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        protected IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IDbContext _dbContext;

        public AuthController(IUserRepository userRepository, IConfiguration configuration, IEmailSender emailSender, IDbContext dbContext)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailSender = emailSender;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseModel), 200)]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            LoginResponseModel result = new LoginResponseModel();

            var user = (await _userRepository.GetLoginDetails(model.Username));



            if (user == null)
                result.CreateFailureResponse("Please check your user name!!");
            else
            {
                var password = UserRepository.GetHashPassword(model.Password, user.Salt);

                if (password != user.Password)
                    result.CreateFailureResponse("Please check your password");
                else
                {
                    if (user.UserTypeID == 8 || user.UserTypeID == 9)
                    {
                        if (!Convert.ToBoolean(user.EmailConfirmed))
                            result.CreateFailureResponse("Your email not confirmed");
                        else if (!Convert.ToBoolean(user.LoginStatus))
                            result.CreateFailureResponse("Your login is inactive");
                        else if (user.IsRejected)
                            result.CreateFailureResponse("Your account is rejected");
                        else if (!user.IsApproved)
                            result.CreateFailureResponse("Your account not approved");
                    }

                    result.AccessToken = await CreateAccessToken(user);
                }
            }

            return Ok(result);
        }

        private async Task<string> CreateAccessToken(UserViewModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(999);

            List<string> roles = (await _userRepository.GetUserRoles(user.UserID)).ToList();

            var claims = new List<Claim>(){
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("UserTypeID", user.UserTypeID.ToString()),
                //new Claim("ProfileURL", user.ProfileImageFileName.ToString()),
                new Claim("PersonalInfoID", user.PersonalInfoID.ToString()),
                //new Claim("EmailAddress", user.EmailAddress),
                //new Claim("MobileNumber", user.MobileNumber),
                //new Claim("UserTypeName", user.UserTypeName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("forgot-password")]
        public async Task<APIBaseResponse> ForgotPassword(ForgotPasswordModel model)
        {
            APIBaseResponse result = new ();

            var user = await _userRepository.GetByEmailAddress(model.EmailAddress);
            if (user == null)
            {
                result.CreateFailureResponse("Invalid Email address!!");
                return result;
            }

            try
            {
                int _min = 1000;
                int _max = 9999;
                Random _rdm = new();
                string OTP = (_rdm.Next(_min, _max)).ToString();

                await _userRepository.UpdateOTP(user.UserID, OTP);

                string brand = "Louvre";
                var message = $@"
                        <div style = ""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2"">
                            <div style = ""margin:50px auto;width:70%;padding:20px 0"">
                                <div style = ""border-bottom:1px solid #eee"" >
                                    <a href = """" style = ""font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600""> {brand} </a>
                                </div>
                                <p style = ""font-size:1.1em""> Hi,</p>
                                <p>Forgot your password? Use the following OTP to complete reset password procedures. </p>
                                <h2 style = ""background: #00466a;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""> {OTP} </h2>       
                                <hr style = ""border:none;border-top:1px solid #eee"" />
                                <div style=""float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
                                    <p> Regards,</p>
                                    <p> team {brand}</p>
                                </div>
                            </div>
                        </div>";
                await _emailSender.SendHtmlEmailAsync(model.EmailAddress, "Password Reset Request", message);

                result.Status = true;
                result.Message = "Please find OTP from your email";
            }
            catch
            {
            }
            return result;
        }


        [HttpPost("reset-password")]
        public async Task<APIBaseResponse> ResetPassword(ResetPasswordModel model)
        {
            APIBaseResponse result = new();

            var user = await _userRepository.GetByEmailAddress(model.EmailAddress);
            if (user == null)
            {
                result.CreateFailureResponse("Invalid Email address!!");
                return result;
            }
            else if(user.SecurityStamp!=model.OTP)
            {
                result.CreateFailureResponse("Invalid OTP!!");
                return result;
            }
            try
            {
                await _userRepository.ResetPassword(user.UserID, model.Password);
                result.Status = true;
                result.Message = "Password has been changed successfully!!";
            }
            catch
            {
            }
            return result;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        public async Task<APIBaseResponse> ChangePassword(ChangePasswordPostModel model)
        {
            var user = await _dbContext.GetAsync<User>(CurrentUserID);

            var currenthashPassword = UserRepository.GetHashPassword(model.CurrentPassword, user.Salt);

            APIBaseResponse result = new ();
            if (user.Password != currenthashPassword)
            {
                result.CreateFailureResponse("Please check your current password!!");
            }
            else
            {
                var hashPassword = UserRepository.GetHashPassword(model.Password,user.Salt);

                await _dbContext.ExecuteAsync($"Update Users Set Password=@HashPassword where UserID=@CurrentUserID", new { HashPassword =hashPassword, CurrentUserID });
                result.Message = "Your Password changed successfully";
            }
            return result;
        }
    }
}
