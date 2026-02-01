using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class Login
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
   [Display(Name = "Email Address")]
      public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        // reCAPTCHA token
        public string? RecaptchaToken { get; set; }
    }

    public class TwoFactorInput
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "Verification code must be 6-7 digits")]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = string.Empty;

        public bool RememberMachine { get; set; }
    }

    public class ChangePassword
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
  [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
        [Validators.StrongPassword]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

 [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ForgotPassword
    {
      [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

   public string? RecaptchaToken { get; set; }
    }

    public class ResetPassword
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
  [DataType(DataType.Password)]
        [Validators.StrongPassword]
     [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
[DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
