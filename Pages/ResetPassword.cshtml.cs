using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class ResetPasswordModel : PageModel
    {
     private readonly UserManager<ApplicationUser> _userManager;
 private readonly IAuditLogService _auditLogService;
        private readonly AuthDbContext _context;
        private readonly ILogger<ResetPasswordModel> _logger;
        private const int PasswordHistoryCount = 2;

     [BindProperty]
     public ResetPasswordViewModel Input { get; set; } = new();

        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }

 public ResetPasswordModel(
      UserManager<ApplicationUser> userManager,
      IAuditLogService auditLogService,
   AuthDbContext context,
            ILogger<ResetPasswordModel> logger)
        {
 _userManager = userManager;
    _auditLogService = auditLogService;
  _context = context;
 _logger = logger;
      }

     public async Task<IActionResult> OnGetAsync(string? userId, string? token)
     {
   if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
       {
  ErrorMessage = "Invalid password reset link.";
       return Page();
       }

 var user = await _userManager.FindByIdAsync(userId);
     if (user == null)
 {
   ErrorMessage = "Invalid password reset link.";
     return Page();
       }

  // Check token expiry
 if (user.PasswordResetTokenExpiry.HasValue && user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
     ErrorMessage = "This password reset link has expired. Please request a new one.";
     return Page();
            }

      Input.UserId = userId;
    Input.Token = token;
    return Page();
  }

        public async Task<IActionResult> OnPostAsync()
        {
    if (!ModelState.IsValid)
   {
 return Page();
          }

    var user = await _userManager.FindByIdAsync(Input.UserId);
    if (user == null)
  {
    ErrorMessage = "Invalid request.";
  return Page();
     }

    // Check token expiry
 if (user.PasswordResetTokenExpiry.HasValue && user.PasswordResetTokenExpiry < DateTime.UtcNow)
       {
     ErrorMessage = "This password reset link has expired. Please request a new one.";
return Page();
        }

     // Check password history (prevent reuse of last 2 passwords)
   var passwordHistories = _context.PasswordHistories
   .Where(ph => ph.UserId == user.Id)
  .OrderByDescending(ph => ph.CreatedAt)
  .Take(PasswordHistoryCount)
    .ToList();

   foreach (var history in passwordHistories)
  {
       var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(
    user, history.PasswordHash, Input.NewPassword);

  if (verificationResult == PasswordVerificationResult.Success ||
   verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
  {
     ModelState.AddModelError("Input.NewPassword",
    $"You cannot reuse any of your last {PasswordHistoryCount} passwords.");
       return Page();
   }
     }

    // Reset the password
     var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.NewPassword);

        if (!result.Succeeded)
        {
     foreach (var error in result.Errors)
     {
     ModelState.AddModelError(string.Empty, error.Description);
  }
         return Page();
            }

     // Store new password in history
      var newHistory = new PasswordHistory
     {
     UserId = user.Id,
    PasswordHash = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword),
        CreatedAt = DateTime.UtcNow
 };
            _context.PasswordHistories.Add(newHistory);

   // Update password change timestamp
  user.PasswordLastChangedAt = DateTime.UtcNow;
   user.PasswordResetTokenExpiry = null; // Invalidate the token
 await _userManager.UpdateAsync(user);

  // Clean up old password history
            var oldHistories = _context.PasswordHistories
  .Where(ph => ph.UserId == user.Id)
          .OrderByDescending(ph => ph.CreatedAt)
   .Skip(PasswordHistoryCount)
    .ToList();
 _context.PasswordHistories.RemoveRange(oldHistories);
 await _context.SaveChangesAsync();

 // Audit log
     await _auditLogService.LogAsync(user.Id, "Password Reset", "Password was reset via email link");

     _logger.LogInformation("User {UserId} reset their password via email link", user.Id);

  StatusMessage = "Your password has been reset successfully. You can now log in with your new password.";
     return Page();
        }
    }
}
