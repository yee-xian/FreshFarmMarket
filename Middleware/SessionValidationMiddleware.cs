using Microsoft.AspNetCore.Identity;
using WebApplication1.Model;

namespace WebApplication1.Middleware
{
    public class SessionValidationMiddleware
    {
 private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
         _next = next;
       _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
      if (context.User.Identity?.IsAuthenticated == true)
            {
     var user = await userManager.GetUserAsync(context.User);
    
       if (user != null)
         {
  var currentSessionId = context.Session.GetString("SessionId");
 
       // Check if session IDs match
             if (!string.IsNullOrEmpty(user.CurrentSessionId) && 
      !string.IsNullOrEmpty(currentSessionId) &&
         user.CurrentSessionId != currentSessionId)
     {
           _logger.LogWarning(
          "Session mismatch detected for user {UserId}. Expected: {Expected}, Got: {Got}",
        user.Id, user.CurrentSessionId, currentSessionId);

 // Sign out the user - session is invalid (logged in elsewhere)
         await signInManager.SignOutAsync();
              context.Session.Clear();

   // ? FIX: Use 'concurrent=1' for concurrent session, not 'sessionExpired'
         // This is different from session timeout
              context.Response.Redirect("/Login?concurrent=1");
   return;
     }
   }
            }

     await _next(context);
        }
    }

    public static class SessionValidationMiddlewareExtensions
    {
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
        {
    return builder.UseMiddleware<SessionValidationMiddleware>();
        }
    }
}
