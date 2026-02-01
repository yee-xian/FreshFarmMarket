# ? SESSION TIMEOUT - 1 MINUTE DEMO CONFIGURATION

## ?? CHANGES MADE

### 1. Program.cs - Session Timeout Updated

**Session Configuration:**
```csharp
options.IdleTimeout = TimeSpan.FromMinutes(1); // ? 1 minute for demo
```

**Cookie Configuration:**
```csharp
options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // ? 1 minute for demo
options.LoginPath = "/Login"; // ? Redirect to /Login on expiry
```

### 2. Audit Logging for Expired Sessions (10% Audit Mark)

When session expires and user tries to access protected page:
```csharp
var auditLog = new AuditLog
{
    UserId = null,
    Action = "Session Expired - Unauthorized Access",
Details = $"Session timeout detected. Unauthorized access attempt to '{requestPath}' from IP: {ipAddress}. User redirected to login.",
    Timestamp = DateTime.Now,
    IpAddress = ipAddress
};
```

### 3. Login Page - Session Expired Message

Added warning message when redirected due to session timeout:
```html
<div class="alert alert-warning">
    <i class="bi bi-clock-history"></i> <strong>Session Expired</strong>
    Your session has timed out due to inactivity. Please log in again.
</div>
```

---

## ?? DEMO TEST STEPS

### Test 1: Session Timeout

1. **Login** with valid credentials
2. **Note the time** - session expires in 1 minute
3. **Wait 1 minute** (don't click anything)
4. **Try to navigate** to any protected page (e.g., Index, ChangePassword)
5. **Expected Results:**
   - ? Redirected to `/Login?sessionExpired=true`
   - ? Yellow warning: "Session Expired - Your session has timed out"
   - ? Audit log entry created

### Test 2: Verify Audit Log Entry

After session expires, check AuditLogs table:
```sql
SELECT * FROM AuditLogs 
WHERE Action = 'Session Expired - Unauthorized Access' 
ORDER BY Timestamp DESC
```

Expected entry:
| Action | Details |
|--------|---------|
| Session Expired - Unauthorized Access | Session timeout detected. Unauthorized access attempt to '/Index' from IP: ::1. User redirected to login. |

---

## ?? CONFIGURATION SUMMARY

| Setting | Value | Description |
|---------|-------|-------------|
| **Session IdleTimeout** | 1 minute | Session expires after 1 min inactivity |
| **Cookie ExpireTimeSpan** | 1 minute | Auth cookie expires after 1 min |
| **SlidingExpiration** | true | Timer resets on activity |
| **LoginPath** | /Login | Redirect destination on expiry |
| **Audit Log** | ? Enabled | Logs "Session Expired - Unauthorized Access" |

---

## ?? IMPORTANT: AFTER DEMO

After your demo, **change back to 30 minutes** for production:

```csharp
// Session Configuration
options.IdleTimeout = TimeSpan.FromMinutes(30); // Production value

// Cookie Configuration  
options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Production value
```

---

## ? BUILD STATUS

```
Build: ? SUCCESSFUL
Session Timeout: ? 1 minute
Audit Logging: ? Enabled
Login Message: ? Shows session expired warning
```

---

**Your session timeout demo is ready!** ??

