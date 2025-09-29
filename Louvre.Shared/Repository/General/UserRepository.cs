
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository
{
    public interface IUserRepository
    {

        #region Claim
        Task<UserViewModel> GetLoginDetails(string userName);
        Task<UserViewModel> GetCurrentClaim(int userId, IDbTransaction tran = null);
        Task<IEnumerable<string>> GetUserRoles(int userId, IDbTransaction tran = null);
        #endregion

        #region Forgot and Reset password and Confirm Email
        Task<UserViewModel> GetByEmailAddress(string email);
        Task<User> GetByIdAndSecurityStamp(int id, string securityStamp);
        Task<string> UpdateSecurityStamp(int userId);
        Task<BaseResponse> ResetPassword(int userId, string password);
        #endregion

        Task<bool> CheckExist(int id, string userName);

        //Task<int> Save(UserPostViewModel data, IDbTransaction transaction = null);

        Task<int> AddNewUserWithMail(User user, string webUrl, IDbTransaction tran);
        Task<BaseResponse> UpdateOTP(int userId, string otp);
    }


    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IEmailSender _emailSender;

        public UserRepository(IDbContext entity, IEmailSender emailSender)
        {
            _dbContext = entity;
            _emailSender = emailSender;
        }

        public static string GetHashPassword(string password, string salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, Encoding.ASCII.GetBytes(salt), KeyDerivationPrf.HMACSHA1, 10000, 32));
        }

        #region Claim

        public async Task<UserViewModel> GetLoginDetails(string userName)
        {
            var query = GetClaimCreationQuery();
            return await _dbContext.GetAsync<UserViewModel>($@"{query} and UserName=@UserName",new { IsDeleted =false, UserName = userName });
        }

        public async Task<UserViewModel> GetCurrentClaim(int userId, IDbTransaction tran = null)
        {
            var query = GetClaimCreationQuery();
            return await _dbContext.GetAsync<UserViewModel>($@"{query} and U.UserID=@UserID", new { IsDeleted = false, UserID = userId}, tran);
        }

        private string GetClaimCreationQuery()
        {
            return $@"Select U.UserID, ISNULL(P.Name,UserName) as Name, UserName,Salt, Password, U.UserTypeID,
			LoginStatus, EmailConfirmed, Isnull(ProfileImageFileName,'') as ProfileImageFileName,U.PersonalInfoID,
            ISNULL(EmailAddress,'') as EmailAddress,ISNULL(MobileNumber,'') as MobileNumber, T.DisplayName as UserTypeName, IsApproved,IsRejected
            from Users U
			LEFT JOIN viPersonalInfos P on U.PersonalInfoID=P.PersonalInfoID
            LEFT JOIN UserTypes T on T.UserTypeID=U.UserTypeID
			where ISNULL(U.IsDeleted,0)=@IsDeleted";
        }

        public async Task<IEnumerable<string>> GetUserRoles(int userId, IDbTransaction tran = null)
        {
            return await _dbContext.GetEnumerableAsync<string>($@"SELECT T.UserTypeName
			FROM Users U
			JOIN UserTypes T on U.UserTypeID=T.UserTypeID
			where UserID={userId}

            UNION

            SELECT Case  T.UserNature when 1 then 'Approver' when 2 then 'Disposal' when 3 then 'Monitor' else T.UserTypeName end
			FROM Users U
			JOIN UserTypes T on U.UserTypeID=T.UserTypeID
			where UserID={userId} and T.UserNature>0

            UNION

            Select M.PermissionName
            From UserModule UM
            JOIN Module M on M.ModuleID = UM.ModuleID
            WHere UM.UserID={userId}", null, tran);
        }

        #endregion

        #region Forgot and Reset password and Confirm Email

        public async Task<User> GetByIdAndSecurityStamp(int id, string securityStamp)
        {
            return await _dbContext.GetAsync<User>($@"Select * From Users 
				where UserId=@UserId and SecurityStamp=@SecurityStamp",new { UserId=id, SecurityStamp=securityStamp });
        }

        public async Task<UserViewModel> GetByEmailAddress(string email)
        {
            return await _dbContext.GetAsync<UserViewModel>($@"Select U.UserID, UserName, Password, EmailAddress, U.UserTypeID, U.PersonalInfoID, LoginStatus, EmailConfirmed, SecurityStamp, SecurityStampIssuedOn,P.Name  
			From Users U
			LEFT JOIN viPersonalInfos P on P.PersonalInfoID=U.PersonalInfoID
			where EmailAddress=@EmailAddress",new { EmailAddress = email });
        }

        public async Task<string> UpdateSecurityStamp(int userId)
        {
            var securityStamp = Guid.NewGuid().ToString();
            await _dbContext.ExecuteAsync($"Update Users set SecurityStamp=@SecurityStamp where UserID=@UserID",new { SecurityStamp =securityStamp, UserID =userId});
            return securityStamp;
        }


        public async Task<BaseResponse> ResetPassword(int userId, string password)
        {
            var salt = Guid.NewGuid().ToString("n").Substring(0, 8);
            var hashPassword = GetHashPassword(password, salt);

            await _dbContext.ExecuteAsync($@"Update Users set Password=@Password,SecurityStamp='',EmailConfirmed=1,Salt=@Salt
			where UserID=@UserID", new { Password =hashPassword, UserID=userId,Salt= salt });

            return new BaseResponse(2);
        }

        public async Task<BaseResponse> UpdateOTP(int userId, string otp)
        {
            await _dbContext.ExecuteAsync($@"Update Users set SecurityStamp=@SecurityStamp
			where UserID=@UserID",new { SecurityStamp=otp, UserID=userId });

            return new BaseResponse(2);
        }

        #endregion


        public async Task<bool> CheckExist(int id, string userName)
        {
            var count = await _dbContext.GetAsync<int>($@"SELECT Count(*)
			FROM  Users
			Where UserID<>@UserID and UserName=@UserName and ISNULL(IsDeleted,0)=@IsDeleted",new { UserID =id, UserName =userName, IsDeleted =false});
            return count > 0 ? true : false;
        }



        //public async Task<int> Save(UserPostViewModel data, IDbTransaction transaction = null)
        //{
        //    User user = null;
        //    if (data.UserID != null)
        //    {
        //        user = await _dbContext.GetAsync<User>(Convert.ToInt32(data.UserID), transaction);
        //    }

        //    if (user == null)
        //    {
        //        user = new();
        //    }
        //    else
        //    {
        //        user = _mapper.Map<UserPostViewModel, User>(data);
        //        user.EmailConfirmed = true;
        //    }

        //    return await _dbContext.SaveAsync<User>(user, transaction);
        //}


        public async Task<int> AddNewUserWithMail(User user, string webUrl, IDbTransaction tran)
        {
            var personalInfo = await _dbContext.GetAsync<PersonalInfo>(Convert.ToInt32(user.PersonalInfoID), tran);
            int userId;
            if (user.UserID == null)
            {
                var u = await _dbContext.GetAsyncByFieldName<User>("UserName", user.UserName, tran);
                if (u != null)
                {
                    throw new PreDefinedException("Username already exist");
                }

                //string password = Guid.NewGuid().ToString("n").Substring(0, 8);
                var securityStamp = Guid.NewGuid().ToString();
                user.SecurityStamp = securityStamp;
                user.SecurityStampIssuedOn = System.DateTime.UtcNow;

                userId = await _dbContext.SaveAsync<User>(user, tran);
                if (user.EmailConfirmed)
                {
                    var message = $@"<p>Dear {personalInfo.FirstName},</p>
                        <p style='padding-left: 20px;'>You can login through {webUrl}/</p>
                        <p style='padding-left: 20px;'><a href='{webUrl}/login'>Login Now</a></p>
                        <p style='padding-left: 20px;'>Login ID : {user.UserName} </p>";
                    await _emailSender.SendHtmlEmailAsync(personalInfo.Email1, "New Login Credential", message, tran);
                }
                else
                {
                    var message = $@"Dear {personalInfo.FirstName},<br>
						<p style='padding-left: 20px;'>Please verify your e-mail address to complete the registration.<br>	
						Your UserID is : {personalInfo.Email1}<br>
						<a href='{webUrl}/account-confirm-email/{userId}/{securityStamp}'>click here to confirm</a></p>";
                    await _emailSender.SendHtmlEmailAsync(personalInfo.Email1, "Verify your e-mail address of your account", message, tran);
                }
            }
            else
            {
                var u = await _dbContext.GetAsyncByFieldName<User>("UserName", user.UserName, tran);
                if (u != null && u.UserID != user.UserID)
                {
                    throw new PreDefinedException("Username already exist");
                }

                var ex_user = await _dbContext.GetAsync<User>(Convert.ToInt32(user.UserID), tran);
                ex_user.UserName = user.UserName;
                ex_user.EmailAddress = user.EmailAddress;
                ex_user.MobileNumber = user.MobileNumber;
                userId = await _dbContext.SaveAsync(ex_user, tran);
            }
            return userId;
        }



    }
}
