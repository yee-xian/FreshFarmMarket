# reCAPTCHA v3 Implementation - Complete Guide

## Fresh Farm Market (IT2163-05) - Anti-Bot Security (5% Marks)

### Overview
This document outlines the complete implementation of Google reCAPTCHA v3 integration across your Fresh Farm Market project, satisfying the Anti-Bot requirement (5%) and contributing to Input Validation & Sanitation (15%).

---

## Architecture Summary

### Three-Layer Implementation

#### Layer 1: Front-End (Client-Side)
- **File**: `Pages/Login.cshtml` & `Pages/Register.cshtml`
- **Technology**: JavaScript + Google reCAPTCHA v3 API
- **Purpose**: Generate security tokens without user interaction
- **Action Labels**: `login` and `registration`

#### Layer 2: Back-End Validation (Server-Side)
- **File**: `Services/RecaptchaValidationService.cs`
- **Technology**: ASP.NET Core + HTTP Client
- **Purpose**: Verify tokens with Google API, validate scores
- **Features**:
  - Score validation (minimum 0.5)
  - Action label verification
  - Detailed error reporting
  - Audit logging of all outcomes

#### Layer 3: Dependency Injection & Middleware
- **File**: `Program.cs`
- **Registrations**:
  ```csharp
  builder.Services.AddHttpClient<IRecaptchaValidationService, RecaptchaValidationService>();
  builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();
  ```

---

## Implementation Details

### 1. Front-End Token Generation

**Location**: `Pages/Login.cshtml` and `Pages/Register.cshtml`

```html
<!-- Load reCAPTCHA Script -->
@if (recaptchaEnabled)
{
    <script src="https://www.google.com/recaptcha/api.js?render=@siteKey"></script>
}

<!-- Hidden Token Field -->
<input type="hidden" id="recaptchaToken" name="LModel.RecaptchaToken" />

<!-- Form Submission Handler -->
<script>
    grecaptcha.ready(function() {
   grecaptcha.execute(siteKey, { action: 'login' }).then(function(token) {
            document.getElementById('recaptchaToken').value = token;
  document.getElementById('loginForm').requestSubmit();
     });
    });
</script>
```

**Key Features**:
- ? Silent verification (no user interaction required)
- ? Action labels: `login` and `registration`
- ? Automatic token generation on form submit
- ? Fallback to manual submission if reCAPTCHA fails
- ? Development mode support (skips verification if not configured)

---

### 2. Back-End Verification Service

**Location**: `Services/RecaptchaValidationService.cs`

#### Core Responsibilities:

1. **Token Validation**
   - Sends token to Google API endpoint
   - Verifies with your Secret Key
   - Handles network errors gracefully

2. **Score Validation**
   - Minimum threshold: 0.5 (configurable)
   - Scores: 0.0 (bot) to 1.0 (human)
   - Logged for audit trails

3. **Action Verification**
 - Confirms action label matches ('login' vs 'registration')
   - Prevents token reuse across endpoints
   - Returns `ACTION_MISMATCH` if violated

4. **Audit Logging**
   ```csharp
   await _auditLogService.LogAsync(userEmail, 
       $"reCAPTCHA Verified - LOGIN",
     $"Score: 0.95, Code: SUCCESS");
   ```

#### Error Handling:

| Error Code | Meaning | Example |
|-----------|---------|---------|
| `MISSING_TOKEN` | No token provided | Network issue on client |
| `LOW_SCORE` | Score < 0.5 | Suspicious behavior detected |
| `ACTION_MISMATCH` | Wrong action label | Token from registration used for login |
| `INVALID_RESPONSE` | Google API parse error | Malformed response |
| `NETWORK_ERROR` | HTTP error to Google | Connection timeout |

---

### 3. Controller/Page Integration

**File**: `Pages/Login.cshtml.cs`

```csharp
var recaptchaResult = await _recaptchaValidator.ValidateTokenAsync(
    LModel.RecaptchaToken, 
    "login",  // Action label
    LModel.Email  // For audit logging
);

if (!recaptchaResult.IsValid)
{
    // Log specific error
_logger.LogWarning("reCAPTCHA failed: {ErrorCode} - {ErrorMessage}",
        recaptchaResult.ErrorCode, 
     recaptchaResult.ErrorMessage);
    
    // Display user-friendly error
    ModelState.AddModelError(string.Empty, 
        recaptchaResult.ErrorMessage ?? "reCAPTCHA validation failed");
    return Page();
}
```

**Similar implementation in**: `Pages/Register.cshtml.cs`

---

## Configuration

### appsettings.json

```json
{
  "ReCaptcha": {
"SiteKey": "YOUR_RECAPTCHA_V3_SITE_KEY",
    "SecretKey": "YOUR_RECAPTCHA_V3_SECRET_KEY"
  }
}
```

### Getting Your Keys (Free):

1. Visit: https://www.google.com/recaptcha/admin
2. Create a new site with:
   - **Type**: reCAPTCHA v3
   - **Domains**: localhost, your-domain.com
3. Copy credentials to `appsettings.json`

### Development Mode:
If keys are not configured or set to `YOUR_RECAPTCHA_V3_*`, the service:
- ? Skips validation (returns `IsValid = true`)
- ? Logs "running in development mode"
- ? Allows testing without Internet connectivity

---

## Audit Logging Integration

### Logged Events:

1. **Successful Verification**
   ```
   Action: reCAPTCHA Verified - LOGIN
   Details: Score: 0.95, Code: SUCCESS
   ```

2. **Low Score Detection**
   ```
   Action: reCAPTCHA Failed - LOGIN
   Details: Score: 0.32, Code: LOW_SCORE
   ```

3. **Action Mismatch**
   ```
   Action: reCAPTCHA Failed - LOGIN
   Details: Code: ACTION_MISMATCH
   ```

4. **Network Errors**
   ```
   Action: reCAPTCHA Failed - LOGIN
   Details: Code: NETWORK_ERROR
   ```

**Database Table**: `AuditLogs`
- **UserId**: User attempting login
- **Action**: reCAPTCHA result
- **Details**: Score + error code
- **Timestamp**: DateTime.Now
- **IpAddress**: Remote IP for geo-location analysis
- **UserAgent**: Browser/device info

---

## Security Benefits

### What reCAPTCHA v3 Prevents:

1. **Credential Stuffing**
   - Detects bots trying multiple passwords
   - Scores low (< 0.5) = likely bot

2. **Brute Force Attacks**
   - Combined with rate limiting (3 failed attempts = 15-min lockout)
   - reCAPTCHA adds ML-based detection layer

3. **Distributed Attacks**
   - Google analyzes patterns across millions of sites
   - Detects suspicious IP addresses/geolocation

4. **Token Hijacking**
   - Tokens are single-use
   - Action labels prevent cross-endpoint reuse
   - 2-minute expiration (Google-side)

---

## Testing Checklist

### Development Testing:

- [ ] Disable keys in `appsettings.json` ? App works without reCAPTCHA
- [ ] Enable keys ? reCAPTCHA badge appears (bottom-right)
- [ ] Submit login ? Token generated automatically
- [ ] Check **AuditLogs** table ? Verification logged with score
- [ ] Failed attempt ? Error message displays gracefully

### Production Testing:

- [ ] reCAPTCHA badge visible
- [ ] Legitimate users score > 0.5 ? Login succeeds
- [ ] Bot attempts ? reCAPTCHA fails gracefully
- [ ] Audit logs record all attempts for compliance

---

## Demo Talking Points (5-7 Minutes)

### What to Highlight:

1. **Badge Location**
   - "You'll notice the reCAPTCHA badge in the bottom-right corner—that's a visible indicator it's active and Google is monitoring for bots."

2. **Silent Verification**
   - "Users don't see a captcha puzzle. reCAPTCHA v3 uses behavioral analysis to verify humanity silently."

3. **Score System**
   - "Google assigns a score (0-1). We set threshold at 0.5—anything below that is flagged as suspicious."

4. **Audit Trail**
   - "Every reCAPTCHA verification is logged with the score. This gives us compliance evidence for security audits."

5. **Graceful Degradation**
   - "In development, without configured keys, the app still works perfectly. This makes testing easier."

---

## File Changes Summary

| File | Changes |
|------|---------|
| `Services/RecaptchaService.cs` | ? Created enhanced validation service |
| `Program.cs` | ? Registered DI services |
| `Pages/Login.cshtml.cs` | ? Updated to use new validator with score checks |
| `Pages/Register.cshtml.cs` | ? Updated to use new validator with score checks |
| `Pages/Login.cshtml` | ? Added reCAPTCHA script + token generation |
| `Pages/Register.cshtml` | ? Added reCAPTCHA script + token generation |

---

## Compliance Checklist

### Rubric Requirements:

- [?] **Anti-Bot (5%)**: reCAPTCHA v3 fully implemented with score validation
- [?] **Input Validation (15%)**: Server-side token verification prevents injection
- [?] **Audit Logging (10%)**: All reCAPTCHA attempts logged with score + error codes
- [?] **Error Handling (10%)**: Graceful error messages + development mode support
- [?] **Demo Ready**: Badge visible, scoring logged, compatible with tutor review

---

## Support & Troubleshooting

### Common Issues:

**Q: Badge not appearing?**
A: Check that `SiteKey` is not `YOUR_RECAPTCHA_V3_SITE_KEY` and reCAPTCHA script is loading.

**Q: All logins fail with "suspicious activity"?**
A: Check `appsettings.json` has correct `SecretKey`. Localhost testing may have low scores due to VPN/proxy.

**Q: Audit logs show `NETWORK_ERROR`?**
A: Verify Internet connectivity to `https://www.google.com/recaptcha/api/siteverify`. Check firewall.

**Q: Why is development working but production failing?**
A: Ensure production domain is added to your reCAPTCHA admin console.

---

## Next Steps

1. **Obtain Keys**: Sign up at https://www.google.com/recaptcha/admin
2. **Configure**: Add keys to `appsettings.json`
3. **Test**: Login attempt ? Check AuditLogs table
4. **Review Logs**: Verify scores are reasonable (0.5+)
5. **Demo**: Show badge, scores, and audit trail to tutor

---

**Implementation Complete ?**

Your Fresh Farm Market project now has enterprise-grade anti-bot protection with full audit compliance!
