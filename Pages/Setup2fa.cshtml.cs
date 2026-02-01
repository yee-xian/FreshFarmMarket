using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;
using QRCoder;
using System.Text;
using System.Text.Encodings.Web;
using WebApplication1.Model;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    [Authorize]
    public class Setup2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLogService;
     private readonly UrlEncoder _urlEncoder;
        private readonly ILogger<Setup2faModel> _logger;
 private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        [TempData]
        public string[]? RecoveryCodes { get; set; }

      [TempData]
        public string? StatusMessage { get; set; }

      [BindProperty]
     public string? Code { get; set; }

        public string? SharedKey { get; set; }
        public string? AuthenticatorUri { get; set; }
        public string? QrCodeImageBase64 { get; set; }
        public bool Is2faEnabled { get; set; }

        public Setup2faModel(
       UserManager<ApplicationUser> userManager,
 IAuditLogService auditLogService,
        UrlEncoder urlEncoder,
         ILogger<Setup2faModel> logger)
        {
            _userManager = userManager;
 _auditLogService = auditLogService;
            _urlEncoder = urlEncoder;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
       {
    return RedirectToPage("/Login");
        }

            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

   if (!Is2faEnabled)
    {
  await LoadSharedKeyAndQrCodeUriAsync(user);
            }

    return Page();
     }

        public async Task<IActionResult> OnPostEnableAsync()
      {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
    return RedirectToPage("/Login");
            }

      if (string.IsNullOrEmpty(Code))
     {
     ModelState.AddModelError("Code", "Verification code is required");
         await LoadSharedKeyAndQrCodeUriAsync(user);
  return Page();
    }

            var verificationCode = Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            // Get the authenticator key
          var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
  if (string.IsNullOrEmpty(unformattedKey))
            {
         ModelState.AddModelError("Code", "Authenticator key not found. Please refresh the page.");
       await LoadSharedKeyAndQrCodeUriAsync(user);
    return Page();
     }

            // Verify using Otp.NET with a verification window of 1 (allows for time drift)
            var isValid = VerifyTotpCode(unformattedKey, verificationCode);

   if (!isValid)
    {
       ModelState.AddModelError("Code", "Verification code is invalid. Please ensure your device time is synchronized.");
      await LoadSharedKeyAndQrCodeUriAsync(user);
     return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

          _logger.LogInformation("User {UserId} enabled 2FA with authenticator app", user.Id);
         await _auditLogService.LogAsync(user.Id, "2FA Enabled", "Two-factor authentication enabled");

  // Generate recovery codes
         var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
    RecoveryCodes = recoveryCodes?.ToArray();

            StatusMessage = "Two-factor authentication has been enabled. Save your recovery codes!";
       return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisableAsync()
    {
     var user = await _userManager.GetUserAsync(User);
        if (user == null)
            {
       return RedirectToPage("/Login");
            }

await _userManager.SetTwoFactorEnabledAsync(user, false);
     await _userManager.ResetAuthenticatorKeyAsync(user);

            _logger.LogInformation("User {UserId} disabled 2FA", user.Id);
            await _auditLogService.LogAsync(user.Id, "2FA Disabled", "Two-factor authentication disabled");

            StatusMessage = "Two-factor authentication has been disabled.";
 return RedirectToPage();
}

   private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
     // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
      {
          await _userManager.ResetAuthenticatorKeyAsync(user);
  unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
   }

            SharedKey = FormatKey(unformattedKey!);
     AuthenticatorUri = GenerateQrCodeUri(user.Email!, unformattedKey!);
       
       // Generate QR code as Base64 image using QRCoder
   QrCodeImageBase64 = GenerateQrCodeBase64(AuthenticatorUri);
        }

        private string FormatKey(string unformattedKey)
   {
       var result = new StringBuilder();
   int currentPosition = 0;
       while (currentPosition + 4 < unformattedKey.Length)
  {
      result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
    currentPosition += 4;
      }
  if (currentPosition < unformattedKey.Length)
  {
            result.Append(unformattedKey.AsSpan(currentPosition));
  }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
   {
      return string.Format(
   AuthenticatorUriFormat,
        _urlEncoder.Encode("FreshFarmMarket"),
 _urlEncoder.Encode(email),
      unformattedKey);
        }

        private string GenerateQrCodeBase64(string text)
        {
      using var qrGenerator = new QRCodeGenerator();
         using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
         using var qrCode = new PngByteQRCode(qrCodeData);
         var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        private bool VerifyTotpCode(string secretKey, string code)
  {
            try
            {
  // Convert the Base32 secret key to bytes
    var keyBytes = Base32Encoding.ToBytes(secretKey);
                
        // Create TOTP instance
      var totp = new Totp(keyBytes);
           
          // Verify with a window of 1 (allows 30 seconds before and after)
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
            catch (Exception ex)
     {
   _logger.LogError(ex, "Error verifying TOTP code");
       return false;
       }
        }
    }
}
