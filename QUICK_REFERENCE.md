# Quick Reference - reCAPTCHA v3 Implementation

## Configuration

**File**: `appsettings.json`

```json
"RecaptchaSettings": {
  "SiteKey": "6LfyCVwsAAAAAPPK-OVXr8nYhfSy0jvIjNZCN18i",
  "SecretKey": "6LfyCVwsAAAAALWxOpTlfSVRdlX0o5tOXSXj7R1Z",
  "MinimumScore": 0.5,
  "Enabled": true
}
```

---

## Frontend (JavaScript)

```javascript
// reCAPTCHA will automatically:
// 1. Load from: https://www.google.com/recaptcha/api.js?render=SITE_KEY
// 2. Generate token on form submit
// 3. Execute: grecaptcha.execute(siteKey, { action: 'login' or 'register' })
// 4. Store in hidden field: <input id="recaptchaToken" ... />
// 5. Submit form with token
```

---

## Backend (C#)

```csharp
// In Login.cshtml.cs OnPostAsync():
var recaptchaResult = await _recaptchaValidator.ValidateTokenAsync(
    LModel.RecaptchaToken,  // From form
    "login",    // Action label
    LModel.Email        // For audit logging
);

if (!recaptchaResult.IsValid)
{
    // Score < 0.5 detected as bot
    ModelState.AddModelError(string.Empty, recaptchaResult.ErrorMessage);
    return Page();  // Block login
}

// Continue with normal login...
```

---

## Audit Logging

Every verification is logged to `AuditLogs`:

```sql
SELECT UserId, Action, Details, Timestamp 
FROM AuditLogs 
WHERE Action LIKE '%reCAPTCHA%' 
ORDER BY Timestamp DESC;
```

**Example entry:**
```
Action: reCAPTCHA Verified - LOGIN
Details: Score: 0.95, ErrorCode: SUCCESS
```

---

## Error Codes

| Code | Meaning | User Message |
|------|---------|--------------|
| `LOW_SCORE` | Score < 0.5 | "Suspicious activity detected. Please try again." |
| `MISSING_TOKEN` | No token | "reCAPTCHA token is missing" |
| `NETWORK_ERROR` | API error | "Network error during verification" |
| `ACTION_MISMATCH` | Wrong action | "reCAPTCHA action verification failed" |

---

## Key Classes

**Settings**: `WebApplication1.Settings.RecaptchaSettings`
**Service**: `WebApplication1.Services.IRecaptchaValidationService`
**Result**: `WebApplication1.Services.RecaptchaValidationResult`

---

## Testing Checklist

- [ ] reCAPTCHA badge visible on Login/Register pages
- [ ] Token generated on form submit
- [ ] Successful login with human score (> 0.5)
- [ ] Blocked login with bot score (< 0.5)
- [ ] Audit logs show verification attempts
- [ ] Development mode works without configured keys

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "reCAPTCHA not loaded" | Check Internet connection |
| All logins blocked | Lower `MinimumScore` in config |
| No audit logs | Check `AuditLogService` registration |
| Build error: `IOptions<>` | Ensure `using Microsoft.Extensions.Options;` |

---

## Google Console

Monitor your reCAPTCHA analytics:
https://www.google.com/recaptcha/admin

**Your Site Key**: `6LfyCVwsAAAAAPPK-OVXr8nYhfSy0jvIjNZCN18i`

---

## Security Reminders

?? **Never expose Secret Key in frontend code**
?? **Always verify tokens server-side**
?? **Log all verification attempts for compliance**
?? **Use HTTPS in production only**

---

**Status**: ? READY TO USE
