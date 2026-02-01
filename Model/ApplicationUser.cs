using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
    [MaxLength(100)]
      public string FullName { get; set; } = string.Empty;

     [Required]
      [MaxLength(10)]
   public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string MobileNo { get; set; } = string.Empty;

        [Required]
   [MaxLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
     public string AboutMe { get; set; } = string.Empty;

    [Required]
        [MaxLength(500)]
        public string EncryptedCreditCard { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? PhotoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

  // For session management - prevent multiple logins
   public string? CurrentSessionId { get; set; }

        // Password Age Policy - track when password was last changed
        public DateTime? PasswordLastChangedAt { get; set; }

        // For password reset token expiry tracking
 public DateTime? PasswordResetTokenExpiry { get; set; }
    }
}
