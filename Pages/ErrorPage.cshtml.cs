using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorPageModel : PageModel
    {
        public new int StatusCode { get; set; }
        public string ErrorTitle { get; set; } = "Error";
        public string ErrorMessage { get; set; } = "An unexpected error occurred.";
        public string IconClass { get; set; } = "bi-exclamation-triangle";
        public string TextColorClass { get; set; } = "text-danger";
        public string BorderClass { get; set; } = "border-danger";
        public bool ShowDetails { get; set; }
        public string? RequestId { get; set; }

        private readonly ILogger<ErrorPageModel> _logger;
        private readonly IAuditLogService _auditLogService;

        public ErrorPageModel(ILogger<ErrorPageModel> logger, IAuditLogService auditLogService)
        {
            _logger = logger;
            _auditLogService = auditLogService;
        }

        public async Task OnGetAsync(int? statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = statusCode ?? HttpContext.Response.StatusCode;
            ShowDetails = !string.IsNullOrEmpty(RequestId);

            SetErrorDetails(StatusCode);

            _logger.LogWarning("Error page displayed. Status: {StatusCode}, RequestId: {RequestId}",
    StatusCode, RequestId);


            // Audit log the error
            await LogErrorToAuditAsync(StatusCode);
        }

        private async Task LogErrorToAuditAsync(int statusCode)
        {
            try
            {
                var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var requestPath = HttpContext.Request.Path.Value ?? "Unknown";
                var referer = HttpContext.Request.Headers.Referer.ToString() ?? "Direct";

                string action = statusCode switch
                {
                    400 => "Bad Request Error",
                    401 => "Unauthorized Access Attempt",
                    403 => "Access Denied (Forbidden)",
                    404 => "Page Not Found",
                    405 => "Method Not Allowed",
                    408 => "Request Timeout",
                    429 => "Rate Limit Exceeded",
                    500 => "Internal Server Error",
                    502 => "Bad Gateway Error",
                    503 => "Service Unavailable",
                    _ => "Unknown Error"
                };

                string details = $"Status: {statusCode} | Path: {requestPath} | Referer: {referer} | RequestId: {RequestId}";

                await _auditLogService.LogAsync(userId, action, details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log error to audit trail. Action: {Action}, StatusCode: {StatusCode}",
      "Error Logging", statusCode);
            }
        }

        private void SetErrorDetails(int code)
        {
            switch (code)
            {
                case 400:
                    ErrorTitle = "Bad Request";
                    ErrorMessage = "The server could not understand your request. Please check your input and try again.";
                    IconClass = "bi-x-circle";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                case 401:
                    ErrorTitle = "Unauthorized";
                    ErrorMessage = "You need to be logged in to access this page. Please sign in and try again.";
                    IconClass = "bi-lock";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                case 403:
                    ErrorTitle = "Access Denied";
                    ErrorMessage = "Sorry, you don't have permission to access this resource.";
                    IconClass = "bi-shield-x";
                    TextColorClass = "text-danger";
                    BorderClass = "border-danger";
                    break;

                case 404:
                    ErrorTitle = "Page Not Found";
                    ErrorMessage = "Sorry, the page you're looking for doesn't exist or has been moved.";
                    IconClass = "bi-question-circle";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                case 405:
                    ErrorTitle = "Method Not Allowed";
                    ErrorMessage = "The requested method is not supported for this resource.";
                    IconClass = "bi-slash-circle";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                case 408:
                    ErrorTitle = "Request Timeout";
                    ErrorMessage = "The server timed out waiting for your request. Please try again.";
                    IconClass = "bi-clock";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                case 429:
                    ErrorTitle = "Too Many Requests";
                    ErrorMessage = "You've made too many requests. Please wait a moment and try again.";
                    IconClass = "bi-hourglass-split";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                case 500:
                    ErrorTitle = "Internal Server Error";
                    ErrorMessage = "Something went wrong on our end. We're working to fix it. Please try again later.";
                    IconClass = "bi-exclamation-triangle";
                    TextColorClass = "text-danger";
                    BorderClass = "border-danger";
                    break;

                case 502:
                    ErrorTitle = "Bad Gateway";
                    ErrorMessage = "The server received an invalid response. Please try again later.";
                    IconClass = "bi-hdd-network";
                    TextColorClass = "text-danger";
                    BorderClass = "border-danger";
                    break;

                case 503:
                    ErrorTitle = "Service Unavailable";
                    ErrorMessage = "The server is temporarily unavailable. Please try again later.";
                    IconClass = "bi-tools";
                    TextColorClass = "text-warning";
                    BorderClass = "border-warning";
                    break;

                default:
                    ErrorTitle = "Unexpected Error";
                    ErrorMessage = "An unexpected error occurred. Please try again or contact support if the problem persists.";
                    IconClass = "bi-exclamation-triangle";
                    TextColorClass = "text-danger";
                    BorderClass = "border-danger";
                    break;
            }
        }
    }
}
