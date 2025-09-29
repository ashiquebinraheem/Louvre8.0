using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Louvre.Shared.Repository;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Louvre.Pages
{
    public class LoginModel : PageModel
    {
        protected IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public LoginModel(IUserRepository userRepository, IConfiguration configuration, HttpClient httpClient)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [Required]
        [Display(Name = "User name")]
        [BindProperty]
        public string? Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [BindProperty]
        public string? Password { get; set; }

        [Display(Name = "Remember me?")]
        [BindProperty]
        public bool RememberMe { get; set; }


        [BindProperty]
        public string? ReturnURL { get; set; }

        public string? SiteKey { get; private set; }

        [BindProperty(Name = "g-recaptcha-response")]
        public string? RecaptchaResponse { get; set; }

        public IActionResult OnGet(string returnurl)
        {
            SiteKey = _configuration["Recaptcha:SiteKey"];
            ReturnURL = returnurl;
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/home");
            }
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!await ValidateRecaptchaAsync(RecaptchaResponse))
            {
                ModelState.AddModelError(string.Empty, "Captcha validation failed");
                return Page();
            }

            var user = (await _userRepository.GetLoginDetails(Username));
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please check your user name!!";
                return Redirect("/login");
            }
            
            else if (!Convert.ToBoolean(user.EmailConfirmed))
            {
                TempData["ErrorMessage"] = "Please verify your Email address!!";
                return Page();
            }
            else if (!Convert.ToBoolean(user.LoginStatus))
            {
                TempData["ErrorMessage"] = "Login is inactive!!";
                return Redirect("/login");
            }
            else 
            {
                var hashPassword = UserRepository.GetHashPassword(Password, user.Salt);
                if (user.Password != hashPassword)
                {
                    TempData["ErrorMessage"] = "Please check your password!!";
                    return Redirect("/login");
                }
            }

            string[] roles = (await _userRepository.GetUserRoles(user.UserID)).ToArray();

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            identity.AddClaim(new Claim("UserID", user.UserID.ToString()));
            identity.AddClaim(new Claim("UserTypeID", user.UserTypeID.ToString()));
            identity.AddClaim(new Claim("ProfileURL", user.ProfileImageFileName.ToString()));
            identity.AddClaim(new Claim("PersonalInfoID", user.PersonalInfoID.ToString()));
            identity.AddClaim(new Claim("EmailAddress", user.EmailAddress));
            identity.AddClaim(new Claim("MobileNumber", user.MobileNumber));
            identity.AddClaim(new Claim("UserTypeName", user.UserTypeName));

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties
            {
                IsPersistent = RememberMe
            };

          //  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);  // Addded Abdul Razack  for Seesion not validated after log out.


            if (string.IsNullOrEmpty(ReturnURL))
                ReturnURL = "/home";

            return Redirect(ReturnURL);
        }


        private async Task<bool> ValidateRecaptchaAsync(string recaptchaResponse)
        {
            var secret = _configuration["Recaptcha:SecretKey"];

            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptchaResponse}",
                null);

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Success == true && result.Score >= 0.5; // For reCAPTCHA v3
        }

        private class RecaptchaResult
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("challenge_ts")]
            public string? ChallengeTs { get; set; }

            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }

            [JsonPropertyName("error-codes")]
            public string?[] ErrorCodes { get; set; }
        }
    }
}