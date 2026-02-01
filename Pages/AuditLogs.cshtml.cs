using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    [Authorize]
    public class AuditLogsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
 private readonly IAuditLogService _auditLogService;

        public IEnumerable<AuditLog>? AuditLogs { get; set; }

   public AuditLogsModel(
    UserManager<ApplicationUser> userManager,
            IAuditLogService auditLogService)
        {
 _userManager = userManager;
     _auditLogService = auditLogService;
    }

   public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
      return RedirectToPage("/Login");
      }

  AuditLogs = await _auditLogService.GetUserLogsAsync(user.Id, 50);
         return Page();
        }
    }
}
