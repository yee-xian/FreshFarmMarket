namespace WebApplication1.Settings
{
    /// <summary>
    /// reCAPTCHA v3 settings configuration
    /// Maps to RecaptchaSettings section in appsettings.json
    /// </summary>
    public class RecaptchaSettings
    {
    /// <summary>
        /// reCAPTCHA v3 Site Key (public - safe to use in frontend)
    /// </summary>
  public string SiteKey { get; set; } = string.Empty;

        /// <summary>
      /// reCAPTCHA v3 Secret Key (private - must be kept secure on backend)
      /// </summary>
public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Minimum score threshold for valid verification (0.0 to 1.0)
        /// Default: 0.5 (50% confidence it's a human)
        /// </summary>
        public double MinimumScore { get; set; } = 0.5;

      /// <summary>
        /// Enable reCAPTCHA verification
        /// </summary>
  public bool Enabled { get; set; } = true;

        /// <summary>
   /// Validation: Ensure required settings are configured
      /// </summary>
        public bool IsConfigured()
        {
    return !string.IsNullOrEmpty(SiteKey) 
        && !string.IsNullOrEmpty(SecretKey)
      && SiteKey != "YOUR_RECAPTCHA_V3_SITE_KEY"
        && SecretKey != "YOUR_RECAPTCHA_V3_SECRET_KEY";
        }
  }
}
