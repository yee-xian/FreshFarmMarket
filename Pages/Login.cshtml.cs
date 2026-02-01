using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLogService;
        private readonly IRecaptchaValidationService _recaptchaValidator;
        private readonly ILogger<LoginModel> _logger;
        private readonly AuthDbContext _context;

        [BindProperty]
        public Login LModel { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLogService,
            IRecaptchaValidationService recaptchaValidator,
            ILogger<LoginModel> logger,
            AuthDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
            _recaptchaValidator = recaptchaValidator;
            _logger = logger;
            _context = context;
        }

        public IActionResult OnGet(string? concurrent = null)
        {
            // Check if user was logged out due to concurrent session
            if (concurrent == "1")
            {
                ErrorMessage = "You have been logged out because your account was accessed from another device or browser.";
            }

            // Check if user is already signed in
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Get current timestamp for accurate logging
            var currentTime = DateTime.Now;
            returnUrl ??= Url.Content("~/");

            // FIXED #1: Validate reCAPTCHA token is provided (required field)
            if (string.IsNullOrEmpty(LModel.RecaptchaToken))
            {
                // Note: We don't log here as user is not yet authenticated
                _logger.LogWarning("reCAPTCHA token is missing for email {Email} at {Time}",
                    LModel.Email ?? "unknown", currentTime);

                ModelState.AddModelError(string.Empty,
                    "Security verification failed (missing reCAPTCHA token). Please refresh the page and try again. If the problem persists, disable browser extensions and clear your cache.");
                return Page();
            }

            // FIXED #2: Validate reCAPTCHA token with Google
            var recaptchaResult = await _recaptchaValidator.ValidateTokenAsync(
                LModel.RecaptchaToken,
                "login",
                LModel.Email);

            if (!recaptchaResult.IsValid)
            {
                _logger.LogWarning("reCAPTCHA validation failed for email {Email}: {ErrorCode} - {ErrorMessage}",
                    LModel.Email, recaptchaResult.ErrorCode, recaptchaResult.ErrorMessage);

                // FIXED #3: Display custom error message (Rubric 5%)
                ModelState.AddModelError(string.Empty,
                    $"Security verification failed: {recaptchaResult.ErrorMessage ?? "reCAPTCHA validation failed"}. Please try again.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(LModel.Email);

            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }

            // Check if account is locked out (automatic recovery after lockout period)
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                if (lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.Now)
                {
                    var remainingTime = (lockoutEnd.Value - DateTimeOffset.Now).TotalMinutes;

                    await _auditLogService.LogAsync(user.Id, "Login Failed - Account Locked",
                        $"Account is locked until {lockoutEnd.Value:yyyy-MM-dd HH:mm:ss}. Remaining time: {remainingTime:F0} minutes",
                        recaptchaResult.Score);  // UPDATED: Pass reCAPTCHA score

                    ModelState.AddModelError(string.Empty,
                        $"Account is locked due to multiple failed attempts. Please try again in {Math.Ceiling(remainingTime)} minutes.");
                    return Page();
                }
                else
                {
                    // Lockout has expired - reset the lockout (automatic recovery)
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    await _userManager.ResetAccessFailedCountAsync(user);
                    await _auditLogService.LogAsync(user.Id, "Account Recovered",
                        "Lockout period expired - account automatically recovered",
                        recaptchaResult.Score);  // UPDATED: Pass reCAPTCHA score
                    _logger.LogInformation("User {Email} lockout expired, account recovered automatically", user.Email);
                }
            }

            var result = await _signInManager.PasswordSignInAsync(
                LModel.Email,
                LModel.Password,
                LModel.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Generate and store unique SessionId for concurrent session detection
                var sessionId = Guid.NewGuid().ToString();
                user.CurrentSessionId = sessionId;
                user.LastLoginAt = currentTime;
                await _userManager.UpdateAsync(user);

                // Store session ID in HTTP session
                HttpContext.Session.SetString("SessionId", sessionId);

                await _auditLogService.LogAsync(user.Id, "Login Success",
                    $"User logged in successfully at {currentTime:yyyy-MM-dd HH:mm:ss}. SessionId: {sessionId.Substring(0, 8)}...",
                    recaptchaResult.Score);  // UPDATED: Pass reCAPTCHA score
                _logger.LogInformation("User {Email} logged in successfully at {Time}", LModel.Email, currentTime);

                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                await _auditLogService.LogAsync(user.Id, "Login - 2FA Required",
                    $"Two-factor authentication required at {currentTime:yyyy-MM-dd HH:mm:ss}",
                    recaptchaResult.Score);  // UPDATED: Pass reCAPTCHA score
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = LModel.RememberMe });
            }

            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                await _auditLogService.LogAsync(user.Id, "Account Locked",
                    $"Account locked at {currentTime:yyyy-MM-dd HH:mm:ss} until {lockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss") ?? "unknown"}",
                    recaptchaResult.Score);  // UPDATED: Pass reCAPTCHA score
                _logger.LogWarning("User {Email} account locked out at {Time}", LModel.Email, currentTime);

                ModelState.AddModelError(string.Empty,
                    "Account locked out due to multiple failed login attempts. Please try again in 15 minutes.");
                return Page();
            }

            // Failed login attempt
            var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            var remainingAttempts = 3 - failedAttempts; // 3 attempts before lockout

            await _auditLogService.LogAsync(user.Id, "Login Failed",
                $"Invalid password at {currentTime:yyyy-MM-dd HH:mm:ss}. Failed attempts: {failedAttempts}, Remaining: {remainingAttempts}",
                recaptchaResult.Score);  // UPDATED: Pass reCAPTCHA score

            if (remainingAttempts > 0)
            {
                ModelState.AddModelError(string.Empty,
                    $"Invalid email or password. {remainingAttempts} attempt(s) remaining before account lockout.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return Page();
        }
    }
}
