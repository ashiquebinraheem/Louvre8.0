using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Repository;
using System;
using System.Threading.Tasks;

namespace Louvre.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(IUserRepository userRepository, IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
        }

        [BindProperty]
        public string? EmailAddress { get; set; }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userRepository.GetByEmailAddress(EmailAddress);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid Email address";
                return Page();
            }

            var url = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            var securityStamp = await _userRepository.UpdateSecurityStamp(Convert.ToInt32(user.UserID));

            try
            {
                var body = $@"
						Dear {user.Name},<br>
						<h3>Reset your password?</h3>
						If you requested a password reset for your account,click the link below.If you didn't make this request,ignore this email<br>	
						<a href='{url}/reset-password/{user.UserID}/{securityStamp}'>Reset Password</a>";
                await _emailSender.SendHtmlEmailAsync(EmailAddress, "Password Reset Request", body);
            }
            catch
            {
            }
            return RedirectToPage("/Account/Login");
        }
    }
}