using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
     public string Email { get; set; } = string.Empty;

        public string? RecaptchaToken { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
    public string UserId { get; set; } = string.Empty;

     [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
  [Display(Name = "New Password")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
 [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$",
    ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
   public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
 [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class VerifyEmailViewModel
    {
     [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
