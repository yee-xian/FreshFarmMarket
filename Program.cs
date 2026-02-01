using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Middleware;
using WebApplication1.Model;
using WebApplication1.Services;
using WebApplication1.Settings;

var builder = WebApplication.CreateBuilder(args);

// ? GITHUB SECURITY: Remove Server header (branded header)
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddRazorPages();

// Database Context
builder.Services.AddDbContext<AuthDbContext>();

// Configure reCAPTCHA Settings from appsettings.json
builder.Services.Configure<RecaptchaSettings>(
    builder.Configuration.GetSection("RecaptchaSettings"));

// Identity Configuration with strong password policy
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings - Strong password policy (12 chars, upper, lower, digit, special)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
  options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 1;

    // ? Lockout settings - 3 failed attempts, 1 MINUTE lockout FOR DEMO TESTING
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // ? 1 minute for demo testing
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Sign in settings
    options.SignIn.RequireConfirmedAccount = false;
 options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// ? SESSION CONFIGURATION - 1 MINUTE TIMEOUT FOR DEMO TESTING
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1); // ? 1 minute for demo testing
 options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = ".FreshFarmMarket.Session";
});

// ? COOKIE CONFIGURATION - 1 MINUTE TIMEOUT FOR DEMO TESTING
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // ? 1 minute for demo testing
    options.SlidingExpiration = true;
    options.LoginPath = "/Login"; // ? Redirect to /Login on session expiry
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/Error/403";
    
    // ? AUDIT LOG: Handle session expiry events (10% Audit Mark)
    options.Events.OnRedirectToLogin = async context =>
    {
 var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var requestPath = context.Request.Path.Value ?? "Unknown";
    
        // ? FIX: Only show "Session Expired" if user WAS authenticated (cookie exists but expired)
        var hasExpiredSession = context.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application") ||
    context.Request.Cookies.ContainsKey(".FreshFarmMarket.Session");
   
        if (hasExpiredSession)
        {
   // Log session expiry to audit log
    var dbContext = context.HttpContext.RequestServices.GetService<AuthDbContext>();
if (dbContext != null)
    {
         var auditLog = new AuditLog
             {
        UserId = null,
        Action = "Session Expired - Unauthorized Access",
         Details = $"Session timeout detected. Unauthorized access attempt to '{requestPath}' from IP: {ipAddress}. User redirected to login.",
        Timestamp = DateTime.Now,
   IpAddress = ipAddress
           };

   dbContext.AuditLogs.Add(auditLog);
    await dbContext.SaveChangesAsync();
   }
      
    // Redirect to login with session expired flag
     var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
   context.Response.Redirect($"/Login?sessionExpired=true&returnUrl={returnUrl}");
     }
     else
        {
         // User was never logged in - just redirect to login without session expired message
  var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Login?returnUrl={returnUrl}");
        }
    };
});

// Rate Limiting Configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
  options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
        Endpoint = "POST:/Login",
   Period = "1m",
      Limit = 5
     },
new RateLimitRule
        {
      Endpoint = "POST:/Register",
            Period = "1m",
            Limit = 3
        },
new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
  Limit = 100
        }
};
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// Custom Services
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// reCAPTCHA Services - Register both the enhanced and legacy services
builder.Services.AddHttpClient<IRecaptchaValidationService, RecaptchaValidationService>();
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>();

// HTTP Context Accessor (for audit logging)
builder.Services.AddHttpContextAccessor();

// Anti-forgery (CSRF protection)
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Data Protection (for encryption keys)
builder.Services.AddDataProtection();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error500");
    // ? GITHUB SECURITY: HSTS (HTTP Strict Transport Security)
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Rate Limiting
app.UseIpRateLimiting();

app.UseRouting();

// Session must be before Authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Custom error handling - catches 403, 404, 500, etc. AFTER authorization
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Custom Session Validation (prevent multiple logins)
app.UseSessionValidation();

// ? GITHUB SECURITY: Comprehensive Security Headers
app.Use(async (context, next) =>
{
    // ? Remove X-Powered-By header (branded header)
 context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("Server");
    
    // ? Prevent XSS attacks
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // ? Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // ? Prevent clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // ? Content Security Policy
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://www.google.com https://www.gstatic.com https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
 "img-src 'self' data: https:; " +
        "frame-src https://www.google.com;");
 
    // ? GITHUB SECURITY: Referrer Policy (prevents leaking URLs)
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
 
    // ? GITHUB SECURITY: Permissions Policy (restrict browser features)
    context.Response.Headers.Append("Permissions-Policy", 
   "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    
    // ? GITHUB SECURITY: Cache Control (prevent sensitive data caching)
    context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "0");

    await next();
});

app.MapRazorPages();

app.Run();
