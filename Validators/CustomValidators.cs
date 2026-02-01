using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApplication1.Validators
{
    /// <summary>
    /// Validates that password meets strong password requirements:
    /// - Minimum 12 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
  /// - At least one digit
    /// - At least one special character
   /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
      private const int MinLength = 12;

      protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
     if (value == null || string.IsNullOrEmpty(value.ToString()))
 {
     return new ValidationResult("Password is required");
            }

  var password = value.ToString()!;
     var errors = new List<string>();

  if (password.Length < MinLength)
   {
    errors.Add($"Password must be at least {MinLength} characters long");
        }

    if (!Regex.IsMatch(password, @"[A-Z]"))
    {
  errors.Add("Password must contain at least one uppercase letter");
   }

     if (!Regex.IsMatch(password, @"[a-z]"))
    {
      errors.Add("Password must contain at least one lowercase letter");
  }

     if (!Regex.IsMatch(password, @"\d"))
   {
 errors.Add("Password must contain at least one digit");
            }

     if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]"))
            {
      errors.Add("Password must contain at least one special character");
    }

     if (errors.Count > 0)
     {
       return new ValidationResult(string.Join(". ", errors));
   }

     return ValidationResult.Success;
  }
    }

    /// <summary>
    /// Validates that uploaded file has an allowed extension
    /// </summary>
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

  public AllowedExtensionsAttribute(string[] extensions)
    {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
      {
    if (value is IFormFile file)
     {
   var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
       
              if (!_extensions.Contains(extension))
            {
      return new ValidationResult($"Only {string.Join(", ", _extensions).ToUpper()} files are allowed");
  }

  // Additional validation: Check file signature (magic bytes) for JPG
   if (extension == ".jpg" || extension == ".jpeg")
  {
               using var reader = new BinaryReader(file.OpenReadStream());
     var headerBytes = reader.ReadBytes(3);
  
     // JPG files start with FF D8 FF
           if (headerBytes.Length < 3 || 
 headerBytes[0] != 0xFF || 
      headerBytes[1] != 0xD8 || 
       headerBytes[2] != 0xFF)
            {
 return new ValidationResult("Invalid JPG file. The file content does not match JPG format.");
         }
  }
    }

      return ValidationResult.Success;
}
 }

    /// <summary>
    /// Validates that uploaded file does not exceed maximum size
   /// </summary>
    public class MaxFileSizeAttribute : ValidationAttribute
    {
   private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
  {
            _maxFileSize = maxFileSize;
        }

  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
      {
       if (value is IFormFile file)
     {
         if (file.Length > _maxFileSize)
 {
                return new ValidationResult($"File size cannot exceed {_maxFileSize / (1024 * 1024)} MB");
         }
      }

   return ValidationResult.Success;
     }
    }
}
