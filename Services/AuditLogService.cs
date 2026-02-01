using WebApplication1.Model;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string? userId, string action, string? details = null, double? recaptchaScore = null);
        Task<IEnumerable<AuditLog>> GetUserLogsAsync(string userId, int count = 50);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly AuthDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(AuthDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuditLogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(string? userId, string action, string? details = null, double? recaptchaScore = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var currentTime = DateTime.Now;

                // FIXED: Handle null or invalid userId gracefully
                string? actualUserId = userId;

                // If userId is email, empty, or "unknown", treat as null (system audit log)
                if (string.IsNullOrEmpty(userId) || userId.Contains("@") || userId == "unknown")
                {
                    actualUserId = null;// Allow null for system/anonymous logs
                }
                else
                {
                    // Verify user exists in database only if userId is not null
                    var userExists = await _context.Users.AnyAsync(u => u.Id == actualUserId);
                    if (!userExists)
                    {
                        _logger.LogWarning($"User {actualUserId} does not exist in database. Creating anonymous audit log for action: {action}");
                        actualUserId = null;  // Convert to null instead of failing
                    }
                }

                var auditLog = new AuditLog
                {
                    UserId = actualUserId,  // Can now be null
                    Action = action,
                    Details = details,
                    RecaptchaScore = recaptchaScore,  // NEW: Store reCAPTCHA score
                    IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    Timestamp = currentTime
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging audit for action: {action}. Message: {ex.Message}");
                // Don't throw - audit logging failure shouldn't break the application
            }
        }

        public async Task<IEnumerable<AuditLog>> GetUserLogsAsync(string userId, int count = 50)
        {
            return await Task.FromResult(
                _context.AuditLogs
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToList()
            );
        }
    }
}
