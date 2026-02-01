using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    /// <summary>
  /// Test page to demonstrate 403 Forbidden error handling.
    /// This page requires Admin role. When accessed by non-admin users,
    /// it will trigger a 403 error that should display the custom error page.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminTestModel : PageModel
    {
        public void OnGet()
        {
    // Page content - only accessible if user has Admin role
        }
 }
}
