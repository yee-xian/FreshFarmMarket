# ?? CUSTOM ERROR HANDLING - DEMO SCRIPT

## ?? WHAT YOU'LL SHOW (5 Minutes Total)

This demo proves your application handles errors professionally with automatic audit logging.

---

## ?? DEMO SEGMENTS

### SEGMENT 1: 404 Page Not Found (1 minute)

**What You Say:**
```
"I've implemented custom error handling for all HTTP status codes. 
Let me demonstrate by trying to access a page that doesn't exist."
```

**Actions:**
```
1. Browser address bar
2. Type: https://localhost:7257/InvalidPage
3. Press Enter
4. Wait for page to load

Expected Result:
- Status code: 404
- Title: "Page Not Found"
- Message: "Sorry, the page you're looking for 
           doesn't exist or has been moved"
- Icon: Question mark
- Color: Warning (yellow/orange)
- Buttons: "Go Home" and "Go Back"
- URL: Still shows /InvalidPage (no weird redirects)
```

**Talking Points:**
```
"Notice three professional elements:
1. The URL doesn't change - users stay on their attempted page
2. The error message is clear and helpful, not technical
3. Fresh Farm Market branding is visible - users know they're 
   in a safe, legitimate application"
```

---

### SEGMENT 2: 403 Forbidden/Access Denied (1 minute)

**What You Say:**
```
"Now let me show what happens when a user tries to access 
something they don't have permission for."
```

**Actions (Choose One):**

**Option A: Try to access admin area (if exists)**
```
1. Type: https://localhost:7257/Admin
2. Press Enter

Expected:
- If you're not admin: 403 page
- Title: "Access Denied"
- Icon: Shield with X
- Color: Danger (red)
- Message: "You don't have permission to access this resource"
```

**Option B: Create temp test**
```
1. In Program.cs, add temporary endpoint:
   app.MapGet("/TestForbidden", context => 
   {
       context.Response.StatusCode = 403;
       return context.Response.WriteAsJsonAsync(null);
   });

2. Restart app
3. Navigate: https://localhost:7257/TestForbidden
```

**Talking Points:**
```
"This is different from the 404:
- Different message (permission, not existence)
- Different icon (shield instead of question mark)
- Different color (red, indicating danger)
- Same professional handling"
```

---

### SEGMENT 3: Audit Logs (2 minutes)

**What You Say:**
```
"The key feature: Every error is automatically logged to the database. 
This shows we're tracking everything for security and compliance."
```

**Actions:**
```
1. Navigate to: https://localhost:7257/Login
2. Login with test account
3. Once logged in, go to: https://localhost:7257/AuditLogs
4. Page loads with your audit history
5. Scroll down to find recent error entries

Look for:
- Action: "Page Not Found" or "Access Denied (Forbidden)"
- Timestamp: Should match when you triggered the error
- Details: Shows the path (/InvalidPage or /Admin)
- IpAddress: Your computer's IP (127.0.0.1 or network IP)
- UserAgent: Your browser information
```

**Point to Each Entry:**
```
"For each error I triggered:

Entry 1: 404 Error
- Action: "Page Not Found"
- Details: "Status: 404 | Path: /InvalidPage | ..."
- Timestamp: 14:30:45
- IP Address: 192.168.1.100

Entry 2: 403 Error  
- Action: "Access Denied (Forbidden)"
- Details: "Status: 403 | Path: /Admin | ..."
- Timestamp: 14:32:10
- IP Address: 192.168.1.100

This demonstrates:
? Every error is tracked
? Timestamp shows when it occurred
? IP address shows where it came from
? Path shows what was accessed
? This creates a complete security audit trail"
```

---

### SEGMENT 4: Code Explanation (1 minute)

**What You Say:**
```
"Here's how it's implemented in the code..."
```

**Actions:**
```
1. Open: Pages/ErrorPage.cshtml.cs
2. Show: LogErrorToAuditAsync() method
3. Highlight these lines:

    private async Task LogErrorToAuditAsync(int statusCode)
    {
        var userId = User?.FindFirst(...)?.Value;
        var requestPath = HttpContext.Request.Path.Value;
  
        string action = statusCode switch
        {
          404 => "Page Not Found",
            403 => "Access Denied (Forbidden)",
     500 => "Internal Server Error",
            // ... handles all status codes
        };
        
        string details = $"Status: {statusCode} | Path: {requestPath} | RequestId: {RequestId}";
   
      await _auditLogService.LogAsync(userId, action, details);
}
```

**Explain:**
```
"When a user encounters an error:
1. ErrorPageModel.OnGetAsync() is called
2. SetErrorDetails() customizes the message
3. LogErrorToAuditAsync() is called
4. The method extracts:
   - Which user (if logged in)
   - What HTTP status code
   - What path was accessed
   - Request ID for debugging
5. All info is saved to AuditLogs table

This happens automatically for every error,
creating a security trail for compliance."
```

---

## ?? EXTENDED DEMO (If Time Permits)

### Segment 5A: Show 500 Error (Optional)

**Actions:**
```
1. In a page model (e.g., Index.cshtml.cs), temporarily add:

   public void OnGet()
   {
  throw new Exception("Test error");
   }

2. Restart application
3. Navigate to that page: https://localhost:7257/
4. See 500 error page

5. Show:
   - Title: "Internal Server Error"
   - Icon: Exclamation mark
   - Message: "Something went wrong on our end"
   - RequestId: Shows request ID for correlation
   - Audit log will show "Internal Server Error" entry

6. Remove the throw statement and restart
```

**Talking Points:**
```
"Even when the application crashes:
- User sees a friendly message, not a stack trace
- Nothing technical is exposed (no file paths, no code)
- RequestId helps developers find the issue in server logs
- Error is logged for monitoring
- User can go back and try again"
```

### Segment 5B: Show Rate Limit 429 (Optional)

**Actions:**
```
1. Program.cs already has rate limiting:
   - 5 login attempts per minute max
   
2. To trigger:
   - Go to /Login
   - Try to login 6 times in quick succession
   - 6th attempt shows 429 page

3. Show:
   - Title: "Too Many Requests"
   - Message: "You've made too many requests"
   - Icon: Hourglass (indicates waiting)
- Audit log shows "Rate Limit Exceeded"
```

**Talking Points:**
```
"This protects against brute force attacks:
- Application tracks failed login attempts
- After 5 in one minute, users are rate limited
- Custom error page explains the issue
- Error is logged for security monitoring
- This prevents automated password guessing attacks"
```

---

## ?? TALKING POINTS SUMMARY

### For 5% - Custom Error Messages
```
"Instead of showing technical error codes or stack traces, 
users see professional, branded error pages. Each error type 
has a custom message that explains the situation in plain language 
and offers helpful next steps (Go Home, Login, etc.)."
```

### For 10% - Audit Logging
```
"Every error is automatically logged to the database with:
- Timestamp (exact moment)
- User ID (if logged in)
- Action type (what kind of error)
- Request path (what they tried to access)
- IP address (where it came from)
- RequestId (for server log correlation)

This creates a complete audit trail for security investigations,
compliance reporting, and monitoring suspicious patterns."
```

### For Security Value
```
"The error handling provides security benefits:
1. Information Disclosure Prevention - no system details exposed
2. Attack Detection - can identify scanning/probing attempts
3. Compliance - complete audit trail for regulations
4. User Experience - friendly messages don't scare users
5. Admin Visibility - can see all problems in one place"
```

---

## ? PRE-DEMO CHECKLIST

Before your demo:

- [ ] Application running: `dotnet run`
- [ ] Can navigate to /ForgotPassword and /Login pages
- [ ] Test account created and can login
- [ ] Can navigate to /AuditLogs after login
- [ ] Read through this script once
- [ ] Practice the steps (take 5 minutes)
- [ ] Know where 404 and 403 pages are
- [ ] Understand the code flow
- [ ] Can explain why errors need logging

---

## ?? TIMING BREAKDOWN

| Part | Time | Content |
|------|------|---------|
| **Intro** | 15 sec | Explain what you're showing |
| **404 Demo** | 30 sec | Navigate to invalid page |
| **403 Demo** | 30 sec | Try forbidden page |
| **Audit Logs** | 2 min | Show logs, point out entries |
| **Code** | 1 min | Show LogErrorToAuditAsync method |
| **Q&A** | Open | Answer questions |
| **TOTAL** | **5 min** | Complete demo |

---

## ?? SCRIPT YOU CAN READ TO TUTOR

```
"Good afternoon. I'd like to demonstrate the custom error 
handling in my Fresh Farm Market application.

One of the rubric requirements is handling failures professionally. 
Rather than showing users technical code or scary error messages, 
I've implemented branded error pages and automatic logging.

Let me show you how it works...

[Navigate to /InvalidPage]

When a user tries to access a page that doesn't exist, instead 
of seeing a technical 404 error, they see this professional 
page with:
- Clear message: 'Page you're looking for doesn't exist'
- Fresh Farm Market branding
- Helpful buttons to navigate
- The original URL is preserved

[Navigate to /AuditLogs after login]

Now here's the important part: Every error is logged to the 
database. When I look at the audit logs, I can see:
- Each error event
- When it happened (timestamp)
- Who accessed it (user ID)
- What they tried to access (path)
- Where they came from (IP address)

This serves multiple purposes:
1. Security - identify attack patterns
2. Compliance - complete audit trail
3. Monitoring - know what problems exist
4. Debugging - RequestID correlates with server logs

The implementation is straightforward...
[Show code]

Every error goes through ErrorPageModel, which:
1. Customizes the user-facing message
2. Logs the event to the database
3. Shows a professional error page

This handles all HTTP status codes - 404, 403, 500, 429, etc.
Each one gets logged and shown professionally.

Questions?"
```

---

## ?? WHAT IF SOMETHING GOES WRONG?

### Error doesn't show up in audit logs?
```
Solution 1: Login first (errors by anonymous users show UserId = null)
Solution 2: Check timestamp - audit logs are ordered by timestamp
Solution 3: Verify database connection by checking /AuditLogs page loads
```

### Can't access /InvalidPage (actual 404)?
```
Solution: That's actually perfect - you're seeing the custom 404 page!
It means the error handling is working correctly.
```

### Audit logs page shows 404?
```
Solution: Make sure you're logged in first
/AuditLogs requires authentication
Navigate: /Login ? Login ? /AuditLogs
```

---

## ?? KEY REQUIREMENTS MET

? **Custom Error Message (5%)**
- Professional error pages for all codes
- No technical details exposed
- User-friendly messaging
- Fresh Farm Market branding

? **Audit Logging (10%)**
- Every error logged to database
- Timestamp on all entries
- User ID captured
- Request path recorded
- IP address tracked
- Complete audit trail

**TOTAL: 15% OF ASSIGNMENT** ?

---

## ?? SUCCESS INDICATORS

Your demo is successful when:
- [ ] 404 error page appears with professional styling
- [ ] 403 error page appears with different message
- [ ] Audit logs show the error entries
- [ ] Can point to specific fields in audit log
- [ ] Can explain the code implementation
- [ ] Can explain security benefits

---

**You're ready to demo!** ???

