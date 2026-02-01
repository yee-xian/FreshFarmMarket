using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Encodings.Web;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditLogService _auditLogService;
        private readonly IRecaptchaValidationService _recaptchaValidator;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<RegisterModel> _logger;
        private readonly AuthDbContext _context;

        [BindProperty]
        public Register RModel { get; set; } = new();

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
         SignInManager<ApplicationUser> signInManager,
            IEncryptionService encryptionService,
            IAuditLogService auditLogService,
       IRecaptchaValidationService recaptchaValidator,
  IWebHostEnvironment environment,
       ILogger<RegisterModel> logger,
       AuthDbContext context)
     {
    _userManager = userManager;
      _signInManager = signInManager;
            _encryptionService = encryptionService;
    _auditLogService = auditLogService;
        _recaptchaValidator = recaptchaValidator;
       _environment = environment;
            _logger = logger;
   _context = context;
        }

  public void OnGet()
        {
   // Clear any stale ModelState errors from previous requests
  ModelState.Clear();
        }

  public async Task<IActionResult> OnPostAsync()
   {
            // ? Log that POST was reached
            Console.WriteLine("========================================");
        Console.WriteLine("POST REACHED - OnPostAsync() called");
            Console.WriteLine($"Email: {RModel.Email}");
            Console.WriteLine("========================================");
     _logger.LogInformation("Registration attempt for email: {Email}", RModel.Email);

            // Get current timestamp and IP address
var currentTime = DateTime.Now;
  var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // ? REQUIREMENT 2: Read reCAPTCHA token from Request.Form
   var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
            Console.WriteLine($"reCAPTCHA token received: {(!string.IsNullOrEmpty(recaptchaToken) ? "Yes (" + recaptchaToken.Length + " chars)" : "No")}");

      // ? REQUIREMENT 2: Validate reCAPTCHA token
double? recaptchaScore = null;
 if (!string.IsNullOrEmpty(recaptchaToken))
     {
  Console.WriteLine("Validating reCAPTCHA token...");
      var recaptchaResult = await _recaptchaValidator.ValidateTokenAsync(
         recaptchaToken, "register", RModel.Email);

    if (!recaptchaResult.IsValid || (recaptchaResult.Score < 0.5))
       {
  Console.WriteLine($"reCAPTCHA FAILED - Score: {recaptchaResult.Score}, Error: {recaptchaResult.ErrorMessage}");
 _logger.LogWarning("reCAPTCHA validation failed for {Email}. Score: {Score}, Error: {Error}",
        RModel.Email, recaptchaResult.Score, recaptchaResult.ErrorMessage);

       ModelState.AddModelError(string.Empty, "Security verification failed. Please try again.");

   // Log security event
          try
    {
     await _auditLogService.LogAsync(
 userId: null,
      action: "Security Alert: reCAPTCHA Failed",
          details: $"reCAPTCHA verification failed for {RModel.Email} from IP: {ipAddress}. Score: {recaptchaResult.Score}");
}
     catch (Exception ex)
      {
     _logger.LogError(ex, "Failed to log reCAPTCHA failure");
     }

 return Page();
      }

          recaptchaScore = recaptchaResult.Score;
  Console.WriteLine($"reCAPTCHA PASSED - Score: {recaptchaScore}");
       }
   else
      {
  // If reCAPTCHA is enabled but no token received, fail
   Console.WriteLine("No reCAPTCHA token received");
   _logger.LogWarning("No reCAPTCHA token received for registration attempt from {Email}", RModel.Email);
          
         ModelState.AddModelError(string.Empty, "Security verification failed. Please try again.");
       return Page();
            }

            // ? REQUIREMENT 3: Check for duplicate email AFTER reCAPTCHA passes
            var existingUser = await _userManager.FindByEmailAsync(RModel.Email);
   if (existingUser != null)
  {
                Console.WriteLine($"DUPLICATE EMAIL DETECTED: {RModel.Email}");
        _logger.LogWarning("Duplicate email registration attempt: {Email} from IP: {IpAddress}", RModel.Email, ipAddress);

       // Add error to ModelState
 ModelState.AddModelError("RModel.Email", "This email is already in use.");
       ModelState.AddModelError(string.Empty, "This email is already registered. Please log in or use forgot password.");

            // ? REQUIREMENT 3: Log as Security Alert in AuditLog
  try
       {
    await _auditLogService.LogAsync(
       userId: null,
           action: "Security Alert: Duplicate Registration Attempt",
    details: $"Blocked duplicate registration for {RModel.Email} from IP: {ipAddress}",
     recaptchaScore: recaptchaScore);
                    Console.WriteLine("Audit log saved: Security Alert - Duplicate Registration");
            }
         catch (Exception ex)
                {
       _logger.LogError(ex, "Failed to log duplicate email audit for {Email}", RModel.Email);
         }

                return Page();
    }

   // Validate Photo manually
            if (RModel.Photo == null || RModel.Photo.Length == 0)
          {
         ModelState.AddModelError("RModel.Photo", "Photo is required");
        }
         else
         {
         var extension = Path.GetExtension(RModel.Photo.FileName).ToLowerInvariant();
        if (extension != ".jpg")
   {
             ModelState.AddModelError("RModel.Photo", "Only .JPG files are allowed");
 }
            }

    // Check ModelState validity
        if (!ModelState.IsValid)
            {
         Console.WriteLine("MODEL STATE INVALID - Validation Errors:");
   foreach (var key in ModelState.Keys)
                {
        var errors = ModelState[key]?.Errors;
       if (errors != null && errors.Count > 0)
        {
        foreach (var error in errors)
        {
           Console.WriteLine($"  - {key}: {error.ErrorMessage}");
               }
      }
     }
     return Page();
    }

       // Handle photo upload
     string? photoPath = null;
 if (RModel.Photo != null && RModel.Photo.Length > 0)
      {
         photoPath = await SavePhotoAsync(RModel.Photo);
 if (photoPath == null)
           {
   ModelState.AddModelError("RModel.Photo", "Failed to upload photo. Please try again.");
           return Page();
        }
            }

        // Encrypt credit card number
         var encryptedCreditCard = _encryptionService.Encrypt(RModel.CreditCardNo);

            // Encode text fields to prevent XSS
            var sanitizedFullName = HtmlEncoder.Default.Encode(RModel.FullName);
       var sanitizedDeliveryAddress = HtmlEncoder.Default.Encode(RModel.DeliveryAddress);
          var sanitizedAboutMe = HtmlEncoder.Default.Encode(RModel.AboutMe);

 var user = new ApplicationUser
            {
       UserName = RModel.Email,
      Email = RModel.Email,
          PhoneNumber = RModel.MobileNo,
       EmailConfirmed = true,
        PhoneNumberConfirmed = true,
      FullName = sanitizedFullName,
  Gender = RModel.Gender,
     MobileNo = RModel.MobileNo,
         DeliveryAddress = sanitizedDeliveryAddress,
    AboutMe = sanitizedAboutMe,
            EncryptedCreditCard = encryptedCreditCard,
        PhotoPath = photoPath,
CreatedAt = currentTime,
     PasswordLastChangedAt = currentTime,
  CurrentSessionId = null,
  LastLoginAt = null
         };

            Console.WriteLine("Creating user in database...");
  var result = await _userManager.CreateAsync(user, RModel.Password);

 if (result.Succeeded)
            {
  Console.WriteLine($"USER CREATED SUCCESSFULLY: {RModel.Email}");
     _logger.LogInformation("User {Email} created successfully", RModel.Email);

       // Store password in history
         try
                {
                 var passwordHistory = new PasswordHistory
   {
            UserId = user.Id,
            PasswordHash = _userManager.PasswordHasher.HashPassword(user, RModel.Password),
        CreatedAt = currentTime
        };
 _context.PasswordHistories.Add(passwordHistory);
     await _context.SaveChangesAsync();
          }
     catch (Exception ex)
                {
        _logger.LogError(ex, "Failed to save password history for user {Email}", RModel.Email);
  }

     // Audit Log for successful registration
           try
                {
          await _auditLogService.LogAsync(
      userId: user.Id,
             action: "Registration Success",
         details: $"Account created for {RModel.Email} from IP: {ipAddress}",
    recaptchaScore: recaptchaScore);
      Console.WriteLine("Audit log saved: Registration Success");
     }
            catch (Exception ex)
                {
  _logger.LogError(ex, "Failed to save audit log for user {Email}", RModel.Email);
   }

    // Generate session ID
  var sessionId = Guid.NewGuid().ToString();
      user.CurrentSessionId = sessionId;
                user.LastLoginAt = currentTime;
 await _userManager.UpdateAsync(user);

            // Store session ID
        HttpContext.Session.SetString("SessionId", sessionId);

     // Sign in the user
             await _signInManager.SignInAsync(user, isPersistent: false);

           Console.WriteLine("REDIRECTING TO INDEX");
         return RedirectToPage("Index");
        }

         // If creation failed, log and display Identity errors
       Console.WriteLine("USER CREATION FAILED - Identity Errors:");
            foreach (var error in result.Errors)
            {
       Console.WriteLine($"  - {error.Code}: {error.Description}");
      ModelState.AddModelError(string.Empty, error.Description);
            }

   _logger.LogWarning("User creation failed for {Email}. Errors: {Errors}",
     RModel.Email,
     string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

     // Audit log for failed registration
            try
  {
      await _auditLogService.LogAsync(
         userId: null,
     action: "Registration Failed",
           details: $"Registration failed for {RModel.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            catch (Exception ex)
            {
   _logger.LogError(ex, "Failed to log registration failure for {Email}", RModel.Email);
}

            return Page();
        }

        private async Task<string?> SavePhotoAsync(IFormFile photo)
    {
   try
            {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "photos");
  if (!Directory.Exists(uploadsFolder))
 {
         Directory.CreateDirectory(uploadsFolder);
  }

     var uniqueFileName = $"{Guid.NewGuid()}.jpg";
   var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
await photo.CopyToAsync(fileStream);
        }

     Console.WriteLine($"Photo saved: {uniqueFileName}");
      return $"/uploads/photos/{uniqueFileName}";
       }
    catch (Exception ex)
            {
     _logger.LogError(ex, "Error saving photo");
      Console.WriteLine($"Photo save error: {ex.Message}");
       return null;
          }
      }
    }
}
