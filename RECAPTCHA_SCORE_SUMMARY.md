# reCAPTCHA Score in Activity Logs - COMPLETE ?

## What You Now Have

### ?? Activity Logs with reCAPTCHA Scores

Your Activity Logs page now displays:

```
????????????????????????????????????????????????????????????????????????????????
? Date & Time     ? Action       ? Details  ? reCAPTCHA Score? IP Address   ?
????????????????????????????????????????????????????????????????????????????????
? 31 Jan 14:30:45 ? Login Success? User logged ?  0.95 ??? 192.168.1.1  ?
?          ?              ? SessionId... ? ?   ?
????????????????????????????????????????????????????????????????????????????????
? 31 Jan 14:28:12 ? Login Failed ? Invalid     ?     0.32 ?? ? 192.168.1.2  ?
?     ?            ? password    ?           ?     ?
????????????????????????????????????????????????????????????????????????????????
? 31 Jan 14:15:30 ? Reg Success  ? User     ?     0.87 ??    ? 192.168.1.3  ?
?                 ?       ? registered  ?     ?   ?
????????????????????????????????????????????????????????????????????????????????
```

### ?? Score Color Coding

**What Each Color Means**:

| Color | Score Range | Meaning | Badge |
|-------|-------------|---------|-------|
| ?? Green | 0.90 - 1.00 | Confirmed Human | `0.95` |
| ?? Blue | 0.70 - 0.89 | Likely Human | `0.87` |
| ?? Yellow | 0.50 - 0.69 | Suspicious | `0.65` |
| ?? Red | 0.00 - 0.49 | Bot Detected | `0.32` |
| ? Gray | None | No Score | `-` |

---

## How It Works

### 1?? User Logs In
```
User enters email & password
        ?
reCAPTCHA validates silently
        ?
Google returns score (e.g., 0.95)
        ?
Authentication attempt (check password)
        ?
? Success or ? Failure
```

### 2?? Score is Captured
```
`LoginModel.OnPostAsync()` receives score
        ?
Calls: LogAsync(userId, "Login Success", details, recaptchaResult.Score)
        ?
Score passed to AuditLogService
```

### 3?? Score is Stored
```
AuditLogService receives: 0.95
   ?
Creates AuditLog object:
{
    UserId = "user-id-guid",
    Action = "Login Success",
    Details = "...",
RecaptchaScore = 0.95,  ? NEW
    Timestamp = DateTime.Now,
    IpAddress = "192.168.1.1"
}
        ?
Saves to database
```

### 4?? Score is Displayed
```
User navigates to /AuditLogs
  ?
Page fetches AuditLogs from database
        ?
Displays score with color badge:
   ? 0.95 (green badge)
        ?
User sees complete security history
```

---

## Features Added

? **Database Column**: `AuditLog.RecaptchaScore (double?)`
? **Service Method**: `LogAsync(..., double? recaptchaScore = null)`
? **Login Integration**: Passes score on all auth events
? **Register Integration**: Passes score on registration
? **Display**: Color-coded badges in Activity Logs
? **Migration**: Database schema updated

---

## Implementation Details

### Files Updated:

```
Model/
  ??? AuditLog.cs
    ??? Added: public double? RecaptchaScore { get; set; }

Services/
  ??? AuditLogService.cs
      ??? Updated: LogAsync(..., double? recaptchaScore = null)
      ??? Added: RecaptchaScore = recaptchaScore

Pages/
  ??? Login.cshtml.cs
  ? ??? Updated: All LogAsync calls pass recaptchaResult.Score
  ?
  ??? Register.cshtml.cs
  ?   ??? Updated: LogAsync call passes recaptchaScore
  ?
  ??? AuditLogs.cshtml
      ??? Added: reCAPTCHA Score column
      ??? Added: Color-coded badge display
```

---

## Example Database Records

### Before (Without Score)
```sql
SELECT Id, UserId, Action, Details, Timestamp FROM AuditLogs
WHERE UserId = 'user-123';

Id  | UserId   | Action        | Details           | Timestamp
1   | user-123 | Login Success | User logged in... | 2025-01-31 14:30:45
2   | user-123 | Login Failed  | Invalid password  | 2025-01-31 14:28:12
```

### After (With Score)
```sql
SELECT Id, UserId, Action, RecaptchaScore, Details, Timestamp FROM AuditLogs
WHERE UserId = 'user-123';

Id  | UserId| Action        | RecaptchaScore | Details           | Timestamp
1   | user-123 | Login Success | 0.95    | User logged in... | 2025-01-31 14:30:45
2   | user-123 | Login Failed  | 0.32           | Invalid password  | 2025-01-31 14:28:12
```

---

## Usage Examples

### Example 1: User Logs In Successfully
```
Action: User enters correct email and password
        ?
reCAPTCHA Score: 0.95 (Confirmed Human)
        ?
Result: ? Login succeeds
    ?
Audit Log: "Login Success" with score 0.95 (GREEN badge)
```

### Example 2: Bot Attempts Login
```
Action: Automated script tries to login
        ?
reCAPTCHA Score: 0.22 (Bot Detected)
    ?
Result: ? Login blocked
        ?
Audit Log: "Login Failed" with score 0.22 (RED badge)
```

### Example 3: Legitimate User from VPN
```
Action: User logs in from VPN
        ?
reCAPTCHA Score: 0.67 (Suspicious Activity)
?
Result: ?? Login succeeds but monitored
   ?
Audit Log: "Login Success" with score 0.67 (YELLOW badge)
```

---

## Security Benefits

??? **Bot Detection**
- Low scores indicate automated attacks
- Can trigger additional verification steps
- Helps prevent brute force attacks

??? **Fraud Prevention**
- Unusual access patterns visible
- VPN/proxy usage detected
- Account takeover prevention

??? **Compliance**
- Complete security audit trail
- Documented verification factors
- Incident investigation support

??? **Intelligence**
- Query logs to find attack patterns
- Monitor user behavior
- Generate security reports

---

## Accessing Activity Logs

**As User**:
1. Login to account
2. Click "Activity Logs" in sidebar/menu
3. View complete login history with scores

**As Administrator**:
```sql
-- Find all failed logins with low scores
SELECT UserId, Action, RecaptchaScore, Timestamp, IpAddress
FROM AuditLogs
WHERE Action LIKE '%Failed%' 
  AND RecaptchaScore < 0.5
ORDER BY Timestamp DESC;
```

---

## Next Step: Apply Migration

To activate this feature in your database:

```bash
cd "C:\Users\Yee Xian\Downloads\WebApplication1\WebApplication1"
dotnet ef database update
```

Then restart your application:
```bash
dotnet run
```

---

## Summary

? **reCAPTCHA scores are now**:
- Captured from Google during authentication
- Stored in database with every audit event
- Displayed in Activity Logs with color badges
- Available for security analysis

? **Your users can see**:
- Their complete login history
- reCAPTCHA risk assessment for each attempt
- Why certain attempts may have been flagged

? **You can analyze**:
- Bot attack patterns
- Suspicious login attempts
- Compromised accounts
- User access behavior

**Status**: Ready to deploy! ??
