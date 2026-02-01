using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditLogService _auditLogService;
        private readonly AuthDbContext _context;
        private readonly ILogger<ChangePasswordModel> _logger;
        private const int PasswordHistoryCount = 2; // Last 2 passwords cannot be reused
        private const int MinPasswordAgeDays = 1; // Minimum 1 day before password can be changed
        private const int MaxPasswordAgeDays = 90; // Maximum 90 days before password must be changed

        [BindProperty]
        public ChangePassword Input { get; set; } = new();

        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public bool PasswordExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuditLogService auditLogService,
            AuthDbContext context,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                CheckPasswordAge(user);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // FIX #2: Use DateTime.Now for accurate local timestamps
            var currentTime = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            // Check minimum password age (prevent frequent changes)
            if (user.PasswordLastChangedAt.HasValue)
            {
                var daysSinceLastChange = (currentTime - user.PasswordLastChangedAt.Value).TotalDays;
                if (daysSinceLastChange < MinPasswordAgeDays)
                {
                    var hoursRemaining = (MinPasswordAgeDays * 24) - (currentTime - user.PasswordLastChangedAt.Value).TotalHours;
                    ModelState.AddModelError(string.Empty,
                        $"You cannot change your password yet. Please wait {Math.Ceiling(hoursRemaining)} more hour(s).");
                    return Page();
                }
            }

            // Check if new password was used before (last 2 passwords)
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
                        $"You cannot reuse any of your last {PasswordHistoryCount} passwords. Please choose a different password.");
                    return Page();
                }
            }

            // Attempt to change the password
            var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Store new password in history with accurate timestamp
            var newPasswordHistory = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword),
                CreatedAt = currentTime
            };
            _context.PasswordHistories.Add(newPasswordHistory);

            // Update password change timestamp with local time
            user.PasswordLastChangedAt = currentTime;
            await _userManager.UpdateAsync(user);

            // Clean up old password history (keep only last N passwords)
            var oldHistories = _context.PasswordHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip(PasswordHistoryCount)
                .ToList();

            _context.PasswordHistories.RemoveRange(oldHistories);
            await _context.SaveChangesAsync();

            // Log the password change with accurate timestamp
            await _auditLogService.LogAsync(user.Id, "Password Changed",
                $"User changed their password at {currentTime:yyyy-MM-dd HH:mm:ss}");

            // Refresh sign-in cookie
            await _signInManager.RefreshSignInAsync(user);

            _logger.LogInformation("User {UserId} changed their password successfully at {Time}", user.Id, currentTime);

            StatusMessage = "Your password has been changed successfully.";
            return Page();
        }

        private void CheckPasswordAge(ApplicationUser user)
        {
            if (user.PasswordLastChangedAt.HasValue)
            {
                // Use DateTime.Now for accurate comparison
                var daysSinceChange = (DateTime.Now - user.PasswordLastChangedAt.Value).TotalDays;
                DaysUntilExpiry = MaxPasswordAgeDays - (int)daysSinceChange;

                if (daysSinceChange >= MaxPasswordAgeDays)
                {
                    PasswordExpired = true;
                    ErrorMessage = "Your password has expired. You must change it now.";
                }
                else if (DaysUntilExpiry <= 14)
                {
                    StatusMessage = $"Your password will expire in {DaysUntilExpiry} day(s). Consider changing it soon.";
                }
            }
        }
    }
}
