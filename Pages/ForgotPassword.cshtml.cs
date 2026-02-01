using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
 public class ForgotPasswordModel : PageModel
    {
     private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        [BindProperty]
        public ForgotPasswordViewModel Input { get; set; } = new();

      public string? StatusMessage { get; set; }

        public ForgotPasswordModel(
     UserManager<ApplicationUser> userManager,
   IEmailService emailService,
   IAuditLogService auditLogService,
     ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
     _emailService = emailService;
    _auditLogService = auditLogService;
      _logger = logger;
        }

  public void OnGet()
    {
        }

        public async Task<IActionResult> OnPostAsync()
        {
      if (!ModelState.IsValid)
            {
   return Page();
     }

   var user = await _userManager.FindByEmailAsync(Input.Email);
        
  // Always show success message to prevent email enumeration attacks
          if (user == null)
       {
 _logger.LogWarning("Password reset requested for non-existent email: {Email}", Input.Email);
             StatusMessage = "If an account exists with that email, a password reset link has been sent.";
    return Page();
            }

     // Generate password reset token
      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
     
    // Set token expiry (1 hour)
          user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
  await _userManager.UpdateAsync(user);

    // Generate reset link
          var resetLink = Url.Page(
     "/ResetPassword",
      pageHandler: null,
        values: new { userId = user.Id, token = token },
 protocol: Request.Scheme);

  // Send email
  await _emailService.SendPasswordResetEmailAsync(user.Email!, resetLink!);

 // Audit log
 await _auditLogService.LogAsync(user.Id, "Password Reset Requested", $"Password reset link sent to {user.Email}");

     _logger.LogInformation("Password reset link sent to {Email}", user.Email);

 StatusMessage = "If an account exists with that email, a password reset link has been sent.";
   return Page();
        }
    }
}
