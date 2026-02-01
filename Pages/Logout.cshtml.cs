using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
   private readonly IAuditLogService _auditLogService;
    private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
  SignInManager<ApplicationUser> signInManager,
       UserManager<ApplicationUser> userManager,
   IAuditLogService auditLogService,
        ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
     _userManager = userManager;
   _auditLogService = auditLogService;
     _logger = logger;
        }

      public async Task<IActionResult> OnPostAsync()
        {
     var user = await _userManager.GetUserAsync(User);
   
            if (user != null)
   {
   // Clear session ID
           user.CurrentSessionId = null;
  await _userManager.UpdateAsync(user);

     // Log the logout
     await _auditLogService.LogAsync(user.Id, "Logout", "User logged out");
       _logger.LogInformation("User {Email} logged out", user.Email);
     }

    // Clear session
   HttpContext.Session.Clear();

 await _signInManager.SignOutAsync();
    
   return RedirectToPage("/Index");
     }
    }
}
