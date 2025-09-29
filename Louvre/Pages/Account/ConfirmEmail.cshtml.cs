using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System.Security;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        protected readonly IDbContext _dbContext;

        public ConfirmEmailModel(IDbContext entity)
        {
            _dbContext = entity;
        }

        public async Task OnGetAsync(int userId, string securityStamp)
        {
            TempData["Success"] = "";
            var user = await _dbContext.GetAsync<User>($@"select * from users where UserID=@UserID",new {UserID=userId, SecurityStamp =securityStamp});
            if (user == null)
            {
                TempData["Message"] = "Invaid link";
            }
            else if(user.SecurityStamp != securityStamp && user.EmailConfirmed==true)
            {
				TempData["Message"] = "Your email already verified";
			}
			else if (user.SecurityStamp != securityStamp && user.EmailConfirmed == false)
			{
				TempData["Message"] = "Invaid link";
			}
			else
            {
                await _dbContext.ExecuteAsync($"Update Users set EmailConfirmed=1,SecurityStamp='' where UserID=@UserID",new { UserID =userId});
                var result = new BaseResponse(2);
                TempData["Message"] = result.ResponseMessage;
                if (result.ResponseCode >= 0)
                {
                    TempData["Success"] = "Success";
                }
            }
        }
    }
}