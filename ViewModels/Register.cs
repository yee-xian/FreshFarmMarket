using System.ComponentModel.DataAnnotations;
using WebApplication1.Validators;

namespace WebApplication1.ViewModels
{
    public class Register
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[89]\d{7}$", ErrorMessage = "Mobile number must be 8 digits starting with 8 or 9")]
        [Display(Name = "Mobile Number")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery Address is required")]
        [StringLength(500, ErrorMessage = "Delivery Address cannot exceed 500 characters")]
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StrongPassword]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credit Card Number is required")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit Card Number must be exactly 16 digits")]
        [Display(Name = "Credit Card Number")]
        public string CreditCardNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "About Me is required")]
        [Display(Name = "About Me")]
        public string AboutMe { get; set; } = string.Empty;

        [Display(Name = "Photo (JPG only)")]
        [AllowedExtensions(new[] { ".jpg" })]
        [MaxFileSize(2 * 1024 * 1024)] // 2MB max
        public IFormFile? Photo { get; set; }

        // reCAPTCHA token
        public string? RecaptchaToken { get; set; }
    }
}
