using GearZone.Application.Abstractions;
using GearZone.Web.Pages.Public.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        [BindProperty]
        public RegisterViewModel RegisterInput { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/";
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            var otherKeys = ModelState.Keys
                .Where(k => !k.StartsWith(nameof(Input) + ".") && k != nameof(Input))
                .ToList();
            foreach (var key in otherKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Validation failed: " + string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                ViewData["ActiveTab"] = "login";
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Username, Input.Password, Input.RememberMe);

            if (result.Status == LoginStatus.Success)
            {
                return LocalRedirect(ReturnUrl ?? "/");
            }

            if (result.Status == LoginStatus.EmailNotConfirmed)
            {
                if (result.UserId != null)
                {
                    var callbackUrl = await GetVerifyEmailUrlAsync(result.UserId);
                    await _authService.SendVerificationEmailAsync(result.UserId, Input.Username, callbackUrl);
                    SuccessMessage = "Your email is not verified. We have sent a new verification link to your email.";
                }
                else
                {
                    ErrorMessage = "Your email is not verified. Please check your inbox.";
                }
                ViewData["ActiveTab"] = "login";
                return Page();
            }

            ErrorMessage = result.Status switch
            {
                LoginStatus.LockedOut => "This account has been locked due to too many failed attempts.",
                LoginStatus.InvalidCredentials => "Invalid username or password.",
                _ => "An error occurred during login."
            };

            ViewData["ActiveTab"] = "login";
            return Page();
        }

        public async Task<IActionResult> OnPostRegisterAsync()
        {
            var otherKeys = ModelState.Keys
                .Where(k => !k.StartsWith(nameof(RegisterInput) + ".") && k != nameof(RegisterInput))
                .ToList();
            foreach (var key in otherKeys)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Validation failed: " + string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                ViewData["ActiveTab"] = "signup";
                return Page();
            }

            var (succeeded, errors, userId) = await _authService.RegisterAsync(
                RegisterInput.FullName,
                RegisterInput.Email,
                RegisterInput.Password
            );

            if (succeeded && userId != null)
            {
                var callbackUrl = await GetVerifyEmailUrlAsync(userId);
                await _authService.SendVerificationEmailAsync(userId, RegisterInput.Email, callbackUrl);
                SuccessMessage = "Registration successful! Please check your email to verify your account.";
                ViewData["ActiveTab"] = "login";
                return Page();
            }

            ErrorMessage = string.Join(" ", errors);
            ViewData["ActiveTab"] = "signup";
            return Page();
        }

        private async Task<string> GetVerifyEmailUrlAsync(string userId)
        {
            var token = await _authService.GenerateEmailConfirmationTokenAsync(userId);
            return Url.Page(
                "VerifyEmail",
                pageHandler: null,
                values: new { userId = userId, token = token },
                protocol: Request.Scheme)!;
        }

        public IActionResult OnPostExternalLogin(string provider)
        {
            var redirectUrl = Url.Page("./Login", pageHandler: "Callback", values: new { ReturnUrl });
            var properties = _authService.GetExternalAuthenticationProperties(provider, redirectUrl!);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return Page();
            }

            var (succeeded, error) = await _authService.HandleExternalLoginCallbackAsync();
            if (succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            ErrorMessage = error ?? "Error during external login.";
            return Page();
        }
    }
}

