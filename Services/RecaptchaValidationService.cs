using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WebApplication1.Settings;

namespace WebApplication1.Services
{
    /// <summary>
    /// Interface for reCAPTCHA v3 validation service
    /// Provides detailed validation results including score and audit information
    /// </summary>
    public interface IRecaptchaValidationService
  {
        Task<RecaptchaValidationResult> ValidateTokenAsync(string token, string action, string userEmail = "");
    }

    /// <summary>
    /// Google reCAPTCHA v3 validation service
    /// Implements server-side verification of reCAPTCHA tokens with audit logging
    /// </summary>
  public class RecaptchaValidationService : IRecaptchaValidationService
    {
        private readonly HttpClient _httpClient;
  private readonly RecaptchaSettings _settings;
        private readonly ILogger<RecaptchaValidationService> _logger;
        private readonly IAuditLogService _auditLogService;
        private const string VerificationEndpoint = "https://www.google.com/recaptcha/api/siteverify";

        public RecaptchaValidationService(
            HttpClient httpClient,
       IOptions<RecaptchaSettings> settings,
            ILogger<RecaptchaValidationService> logger,
  IAuditLogService auditLogService)
        {
          _httpClient = httpClient;
         _settings = settings.Value;
            _logger = logger;
     _auditLogService = auditLogService;

 if (!_settings.IsConfigured())
     {
  _logger.LogWarning("reCAPTCHA is not properly configured. Running in development mode.");
     }
 }

        /// <summary>
        /// Validates a reCAPTCHA v3 token by verifying with Google's API
  /// Includes score validation, action verification, and audit logging
        /// </summary>
 public async Task<RecaptchaValidationResult> ValidateTokenAsync(string token, string action, string userEmail = "")
        {
            var result = new RecaptchaValidationResult();

            // Development mode: skip validation if not configured
            if (!_settings.IsConfigured() || !_settings.Enabled)
    {
        _logger.LogInformation("reCAPTCHA validation skipped (development mode or disabled)");
                result.IsValid = true;
             result.IsConfigured = false;
         return result;
  }

         // Validate token presence
            if (string.IsNullOrEmpty(token))
       {
        _logger.LogWarning("reCAPTCHA token is empty for action: {Action}, email: {Email}", action, userEmail);
      result.IsValid = false;
    result.ErrorMessage = "reCAPTCHA token is missing";
           result.ErrorCode = "MISSING_TOKEN";
             await LogAuditAsync(userEmail, action, result);
             return result;
      }

        try
         {
      // Prepare verification request to Google
           var content = new FormUrlEncodedContent(new[]
           {
       new KeyValuePair<string, string>("secret", _settings.SecretKey),
new KeyValuePair<string, string>("response", token)
             });

   // Send verification request
var response = await _httpClient.PostAsync(VerificationEndpoint, content);

    if (!response.IsSuccessStatusCode)
 {
      _logger.LogError("reCAPTCHA API returned status {StatusCode} for action: {Action}, email: {Email}",
         response.StatusCode, action, userEmail);
     result.IsValid = false;
   result.ErrorMessage = "Failed to verify with reCAPTCHA service";
             result.ErrorCode = $"HTTP_{response.StatusCode}";
         await LogAuditAsync(userEmail, action, result);
    return result;
       }

  // Parse Google's response
var apiResponse = await response.Content.ReadFromJsonAsync<GoogleRecaptchaResponse>();

            if (apiResponse == null)
   {
      _logger.LogError("Failed to deserialize reCAPTCHA response for action: {Action}, email: {Email}", action, userEmail);
result.IsValid = false;
           result.ErrorMessage = "Invalid response from reCAPTCHA service";
               result.ErrorCode = "INVALID_RESPONSE";
          await LogAuditAsync(userEmail, action, result);
     return result;
                }

    // Populate result with Google's data
        result.Score = apiResponse.Score;
      result.Action = apiResponse.Action;
      result.ChallengeTimestamp = apiResponse.ChallengeTs;
         result.Hostname = apiResponse.Hostname;

      // Validate Google's success flag
   if (!apiResponse.Success)
 {
         _logger.LogWarning(
     "reCAPTCHA validation failed for action: {Action}, email: {Email}. Errors: {ErrorCodes}",
       action, userEmail, string.Join(", ", apiResponse.ErrorCodes ?? Array.Empty<string>()));

         result.IsValid = false;
          result.ErrorMessage = "reCAPTCHA validation failed";
         result.ErrorCode = string.Join(",", apiResponse.ErrorCodes ?? Array.Empty<string>());
           await LogAuditAsync(userEmail, action, result);
         return result;
 }

    // Validate action matches expected value
    if (!string.Equals(apiResponse.Action, action, StringComparison.OrdinalIgnoreCase))
                {
     _logger.LogWarning(
            "reCAPTCHA action mismatch for email: {Email}. Expected: {Expected}, Got: {Received}",
      userEmail, action, apiResponse.Action);

        result.IsValid = false;
    result.ErrorMessage = "reCAPTCHA action verification failed";
        result.ErrorCode = "ACTION_MISMATCH";
        await LogAuditAsync(userEmail, action, result);
 return result;
                }

  // Validate score threshold
          if (apiResponse.Score < _settings.MinimumScore)
        {
      _logger.LogWarning(
      "reCAPTCHA score too low for action: {Action}, email: {Email}. Score: {Score}, Threshold: {Threshold}",
         action, userEmail, apiResponse.Score, _settings.MinimumScore);

       result.IsValid = false;
 result.ErrorMessage = $"Suspicious activity detected (Score: {apiResponse.Score:F2}). Please try again.";
    result.ErrorCode = "LOW_SCORE";
         await LogAuditAsync(userEmail, action, result);
   return result;
    }

         // Success!
result.IsValid = true;
       _logger.LogInformation(
          "reCAPTCHA validation successful for action: {Action}, email: {Email}. Score: {Score}",
     action, userEmail, apiResponse.Score);

                await LogAuditAsync(userEmail, action, result);
      return result;
    }
        catch (HttpRequestException ex)
   {
       _logger.LogError(ex, "HTTP error during reCAPTCHA validation for action: {Action}, email: {Email}", action, userEmail);
   result.IsValid = false;
  result.ErrorMessage = "Network error during verification. Please try again.";
  result.ErrorCode = "NETWORK_ERROR";
            await LogAuditAsync(userEmail, action, result);
        return result;
       }
         catch (Exception ex)
       {
     _logger.LogError(ex, "Unexpected error during reCAPTCHA validation for action: {Action}, email: {Email}", action, userEmail);
                result.IsValid = false;
           result.ErrorMessage = "Unexpected error during verification. Please try again.";
                result.ErrorCode = "UNKNOWN_ERROR";
 await LogAuditAsync(userEmail, action, result);
      return result;
    }
        }

        /// <summary>
        /// Logs reCAPTCHA validation results to audit log
        /// </summary>
    private async Task LogAuditAsync(string userEmail, string action, RecaptchaValidationResult result)
      {
            try
     {
                string auditAction = result.IsValid
  ? $"reCAPTCHA Verified - {action.ToUpper()}"
         : $"reCAPTCHA Failed - {action.ToUpper()}";

     string details = $"Score: {result.Score:F2}, ErrorCode: {result.ErrorCode ?? "SUCCESS"}";

       await _auditLogService.LogAsync(userEmail ?? "unknown", auditAction, details);
            }
        catch (Exception ex)
    {
         _logger.LogError(ex, "Failed to log reCAPTCHA audit for email: {Email}", userEmail);
  // Don't throw - audit failure shouldn't break the flow
            }
 }
    }

    /// <summary>
    /// Result of reCAPTCHA validation
    /// </summary>
    public class RecaptchaValidationResult
    {
        /// <summary>Whether the token validation was successful</summary>
        public bool IsValid { get; set; }

  /// <summary>Whether reCAPTCHA is configured (false = development mode)</summary>
        public bool IsConfigured { get; set; } = true;

        /// <summary>The reCAPTCHA score (0.0 = bot, 1.0 = human)</summary>
        public double Score { get; set; }

        /// <summary>The action label that was verified</summary>
        public string? Action { get; set; }

        /// <summary>Timestamp from Google API when challenge was issued</summary>
        public DateTime ChallengeTimestamp { get; set; }

     /// <summary>Hostname that Google detected</summary>
    public string? Hostname { get; set; }

        /// <summary>User-friendly error message if validation failed</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Error code for debugging (MISSING_TOKEN, LOW_SCORE, ACTION_MISMATCH, etc.)</summary>
        public string? ErrorCode { get; set; }
    }

    /// <summary>
    /// Google reCAPTCHA API response model
    /// Maps JSON response from Google's verification endpoint
    /// </summary>
    internal class GoogleRecaptchaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("score")]
        public double Score { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("action")]
  public string? Action { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

      [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }

    /// <summary>
    /// Backward compatibility interface
 /// Wraps IRecaptchaValidationService for simple true/false validation
    /// </summary>
    public interface IRecaptchaService
    {
        Task<bool> ValidateAsync(string token);
    }

    /// <summary>
    /// Backward compatibility service
    /// Simple wrapper around IRecaptchaValidationService
    /// </summary>
    public class RecaptchaService : IRecaptchaService
  {
   private readonly IRecaptchaValidationService _validationService;

 public RecaptchaService(IRecaptchaValidationService validationService)
        {
     _validationService = validationService;
        }

        public async Task<bool> ValidateAsync(string token)
        {
var result = await _validationService.ValidateTokenAsync(token, "legacy");
return result.IsValid;
        }
    }
}
