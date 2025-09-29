using Louvre.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    [Authorize]
    public class ChangePasswordModel : BasePageModel
    {
        private readonly IDbContext _dbContext;
        protected IUserRepository _userRepository;

        public ChangePasswordModel(IDbContext dbContext, IUserRepository userRepository)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
        }

        [BindProperty]
        public ChangePasswordPostModel Data { get; set; }

        public async Task<IActionResult> OnPostSaveAsync()
        {
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

            var user = await _dbContext.GetAsync<User>(CurrentUserID);
            var currenthashPassword = UserRepository.GetHashPassword(Data.CurrentPassword, user.Salt);

            BaseResponse result = new BaseResponse();
            
            if (user.Password != currenthashPassword)
            {
                result.CreatErrorResponse(-4);
            }
            else
            {
                user.Salt = Guid.NewGuid().ToString("n").Substring(0, 8);
                var hashPassword = UserRepository.GetHashPassword(Data.Password, user.Salt);

                await _dbContext.ExecuteAsync($"Update Users Set Password=@Password,Salt=@Salt where UserID={CurrentUserID}", new { Password= hashPassword, Salt=user.Salt });
                result.CreatSuccessResponse(3);
            }
            return new JsonResult(result);

        }
    }
}