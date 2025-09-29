using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Louvre.Shared.Models;
using Louvre.Shared.Repository;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {

        private readonly IUserRepository _userRepository;

        public ResetPasswordModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string? Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        public async Task<ActionResult> OnPostAsync(int userId, string securityStamp)
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

            var user = await _userRepository.GetByIdAndSecurityStamp(userId, securityStamp);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid Link";
                return Page();
            }
            else
            {
                var result = await _userRepository.ResetPassword(userId, Password);
                return RedirectToPage("/Account/Login");
            }
        }
    }
}