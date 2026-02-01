using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class LoginWith2faModel : PageModel
  {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
      private readonly IAuditLogService _auditLogService;
     private readonly ILogger<LoginWith2faModel> _logger;

     [BindProperty]
        public TwoFactorInput Input { get; set; } = new();

 public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }

  public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
   IAuditLogService auditLogService,
     ILogger<LoginWith2faModel> logger)
        {
    _signInManager = signInManager;
    _userManager = userManager;
 _auditLogService = auditLogService;
 _logger = logger;
     }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
{
            // Ensure we have a user with 2FA pending
     var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
          if (user == null)
{
 return RedirectToPage("./Login");
      }

   ReturnUrl = returnUrl;
  RememberMe = rememberMe;

       return Page();
 }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
      {
  returnUrl ??= Url.Content("~/");

     if (!ModelState.IsValid)
     {
   return Page();
            }

     var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
         if (user == null)
     {
     return RedirectToPage("./Login");
  }

  var authenticatorCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

  var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
       authenticatorCode,
     rememberMe,
        Input.RememberMachine);

      if (result.Succeeded)
      {
       // Update session ID
   var sessionId = Guid.NewGuid().ToString();
      user.CurrentSessionId = sessionId;
   user.LastLoginAt = DateTime.UtcNow;
   await _userManager.UpdateAsync(user);
    HttpContext.Session.SetString("SessionId", sessionId);

       await _auditLogService.LogAsync(user.Id, "Login - 2FA Success", "Two-factor authentication completed");
            _logger.LogInformation("User {UserId} logged in with 2FA", user.Id);

       return LocalRedirect(returnUrl);
     }

   if (result.IsLockedOut)
   {
      await _auditLogService.LogAsync(user.Id, "Login - 2FA Lockout", "Account locked after failed 2FA attempts");
  _logger.LogWarning("User {UserId} account locked out after 2FA failure", user.Id);
     
      return RedirectToPage("./Lockout");
     }

     await _auditLogService.LogAsync(user.Id, "Login - 2FA Failed", "Invalid 2FA code entered");
  ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
    return Page();
        }
    }
}
