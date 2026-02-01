using System.Net;
using System.Net.Mail;

namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendVerificationCodeAsync(string toEmail, string code);
        Task Send2FACodeAsync(string toEmail, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Fresh Farm Market - Password Reset Request";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
     body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; border: 1px solid #ddd; }}
        .btn {{ display: inline-block; padding: 12px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .btn:hover {{ background-color: #218838; }}
   .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; background-color: #f0f0f0; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .warning {{ color: #d9534f; font-weight: bold; }}
        .expiry {{ color: #666; font-size: 14px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
         <h1>?? Fresh Farm Market</h1>
        </div>
        <div class='content'>
        <h2>Password Reset Request</h2>
            <p>Hello,</p>
            <p>We received a request to reset your password for your Fresh Farm Market account.</p>
            <p>Click the button below to reset your password:</p>
    <p style='text-align: center;'>
     <a href='{resetLink}' class='btn'>Reset Password</a>
</p>
            <p>Or copy and paste this link into your browser:</p>
      <p style='word-break: break-all; color: #007bff; font-size: 12px;'>{resetLink}</p>
            <p class='expiry'><strong>? This link will expire in 1 hour.</strong></p>
            <p>If you did not request this password reset, please ignore this email or contact our support team immediately.</p>
   <p><span class='warning'>?? For your security:</span></p>
            <ul>
      <li>Never share this link with anyone</li>
          <li>Use a strong password (12+ characters with uppercase, lowercase, numbers, and special characters)</li>
              <li>If you don't recognize this request, change your password immediately</li>
            </ul>
        </div>
        <div class='footer'>
     <p>&copy; {DateTime.Now.Year} Fresh Farm Market. All rights reserved.</p>
<p>This is an automated message, please do not reply to this email.</p>
     <p>For support, contact: support@freshfarmmarket.com</p>
  </div>
    </div>
</body>
</html>";

   await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendVerificationCodeAsync(string toEmail, string code)
  {
    var subject = "Fresh Farm Market - Your Verification Code";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
   .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
     .content {{ padding: 30px; background-color: #f9f9f9; border: 1px solid #ddd; }}
        .code {{ font-size: 32px; font-weight: bold; text-align: center; letter-spacing: 8px; color: #28a745; padding: 20px; background-color: #fff; border: 2px dashed #28a745; margin: 20px 0; border-radius: 5px; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; background-color: #f0f0f0; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .expiry {{ color: #d9534f; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
       <h1>?? Fresh Farm Market</h1>
        </div>
        <div class='content'>
<h2>Verification Code</h2>
      <p>Hello,</p>
            <p>Your verification code for Fresh Farm Market is:</p>
          <div class='code'>{code}</div>
       <p><span class='expiry'>? This code will expire in 10 minutes.</span></p>
  <p>If you did not request this code, please ignore this email and do not share it with anyone.</p>
  </div>
     <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Fresh Farm Market. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

         await SendEmailAsync(toEmail, subject, body);
  }

        public async Task Send2FACodeAsync(string toEmail, string code)
        {
var subject = "Fresh Farm Market - Two-Factor Authentication Code";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
     .header {{ background-color: #ffc107; color: #333; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 30px; background-color: #f9f9f9; border: 1px solid #ddd; }}
 .code {{ font-size: 32px; font-weight: bold; text-align: center; letter-spacing: 8px; color: #ffc107; padding: 20px; background-color: #fff; border: 2px dashed #ffc107; margin: 20px 0; border-radius: 5px; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; background-color: #f0f0f0; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .warning {{ color: #d9534f; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Two-Factor Authentication</h1>
        </div>
        <div class='content'>
     <h2>Your Login Code</h2>
   <p>Hello,</p>
            <p>Your two-factor authentication code is:</p>
          <div class='code'>{code}</div>
   <p><strong>? This code will expire in 5 minutes.</strong></p>
   <p><span class='warning'>?? IMPORTANT:</span> If you did not attempt to log in, someone may be trying to access your account. <strong>Change your password immediately</strong>.</p>
    </div>
  <div class='footer'>
     <p>&copy; {DateTime.Now.Year} Fresh Farm Market. All rights reserved.</p>
        </div>
  </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
}

  private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
  {
            try
            {
    var smtpHost = _configuration["Email:SmtpHost"];
   var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "2525");
       var username = _configuration["Email:Username"];
     var password = _configuration["Email:Password"];
                var fromAddress = _configuration["Email:FromAddress"] ?? "noreply@freshfarmmarket.com";
            var fromName = _configuration["Email:FromName"] ?? "Fresh Farm Market";

    // Check if SMTP is configured
     if (string.IsNullOrEmpty(smtpHost) || smtpHost == "YOUR_SMTP_HOST")
{
       _logger.LogWarning("SMTP not configured. Email would be sent to: {Email}", toEmail);
            _logger.LogWarning("Subject: {Subject}", subject);
  _logger.LogWarning("Body preview: HTML email with password reset link");
        await Task.CompletedTask;
 return;
     }

    // Create SMTP client
      using var client = new SmtpClient(smtpHost, smtpPort)
    {
         Credentials = new NetworkCredential(username, password),
    EnableSsl = true,
       Timeout = 10000
    };

     // Create mail message
   var mailMessage = new MailMessage
         {
    From = new MailAddress(fromAddress, fromName),
        Subject = subject,
       Body = htmlBody,
          IsBodyHtml = true
   };

     mailMessage.To.Add(toEmail);

      // Send email
  await client.SendMailAsync(mailMessage);

       _logger.LogInformation("? Email sent successfully to {ToEmail} via Mailtrap", toEmail);
    _logger.LogInformation("Subject: {Subject}", subject);
            }
   catch (SmtpException smtpEx)
        {
                _logger.LogError(smtpEx, "? SMTP Error sending email to {ToEmail}: {ErrorMessage}", toEmail, smtpEx.Message);
  // Don't throw - email failure shouldn't break the application
            }
   catch (Exception ex)
            {
                _logger.LogError(ex, "? Error sending email to {ToEmail}: {ErrorMessage}", toEmail, ex.Message);
    // Don't throw - email failure shouldn't break the application
  }
        }
    }
}
