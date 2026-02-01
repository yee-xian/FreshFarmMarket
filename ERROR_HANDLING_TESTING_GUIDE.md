# ?? ERROR HANDLING - COMPREHENSIVE TESTING GUIDE

## ? QUICK VERIFICATION (5 minutes)

### Setup
```bash
dotnet build
dotnet run
```

### Test Each Error Code

#### Test 1: 404 Not Found
```
Navigate: https://localhost:7257/InvalidPage
Expected: See 404 error page
- Title: "Page Not Found"
- Message: "page you're looking for doesn't exist"
- Icon: Question mark (yellow)
- Buttons: "Go Home", "Go Back"
Audit Check: /AuditLogs shows "Page Not Found" entry
```

#### Test 2: 403 Forbidden
```
Login as non-admin user
Navigate: https://localhost:7257/AdminTest
Expected: See 403 error page
- Title: "Access Denied"
- Message: "don't have permission"
- Icon: Shield with X (red)
- Buttons: "Login", "Go Home", "Go Back"
Audit Check: /AuditLogs shows "Access Denied (Forbidden)" entry
```

#### Test 3: 500 Internal Error
```
1. Open Pages/Index.cshtml.cs
2. Add: throw new Exception("Test");
3. Save and navigate to /Index
Expected: See 500 error page
- Title: "Internal Server Error"
- Message: "something went wrong on our end"
- Icon: Exclamation mark (red)
- RequestId shown
Audit Check: /AuditLogs shows "Internal Server Error" entry
4. Remove the throw statement
```

#### Test 4: 429 Rate Limited
```
1. Go to: /Login
2. Try login 6+ times rapidly
Expected: See 429 error page
- Title: "Too Many Requests"
- Message: "made too many requests"
- Icon: Hourglass (yellow)
Audit Check: /AuditLogs shows "Rate Limit Exceeded" entry
```

---

## ?? VERIFICATION CHECKLIST

- [ ] Build succeeds: `dotnet build`
- [ ] 404 error page displays professionally
- [ ] 403 error page displays professionally
- [ ] Error pages use Fresh Farm Market layout
- [ ] Error pages have proper icons and colors
- [ ] Audit log shows entries for each error type
- [ ] Audit log includes timestamp
- [ ] Audit log includes user ID (if logged in)
- [ ] Audit log includes request path
- [ ] Audit log includes IP address
- [ ] Navigation buttons work on error pages
- [ ] Error details are customized per code

---

## ?? WHAT TO VERIFY

### Code Configuration
? Program.cs has `options.AccessDeniedPath = "/Error/403"`
? Program.cs has `app.UseStatusCodePagesWithReExecute("/Error/{0}")`
? UseStatusCodePages is AFTER UseAuthorization()

### Error Page Handler
? Error.cshtml.cs has LogErrorAsync() method
? SetErrorDetails() maps all error codes
? Audit log is called for each error

### Views
? Error.cshtml has @page "{statusCode?}"
? Error.cshtml uses _Layout.cshtml
? Error.cshtml displays dynamic title, message, icon

### Database
? AuditLogs table has entries for errors
? Entries have Action = error code description
? Entries have Status Code in Details
? Entries have Path in Details
? Entries have Timestamp

---

## ?? TROUBLESHOOTING

### Issue: Only seeing 404 page for all errors?
```
Check: UseStatusCodePages middleware order
Should be: AFTER UseAuthorization(), not before
```

### Issue: Audit log not recording?
```
Check: ErrorModel has access to IAuditLogService
Verify: Service is injected in constructor
Ensure: _auditLogService is not null
```

### Issue: Error page not using layout?
```
Check: Error.cshtml has Layout = "~/Pages/Shared/_Layout.cshtml"
Or: Just use @page at top (inherits layout automatically)
```

### Issue: 403 not redirecting?
```
Check: AccessDeniedPath = "/Error/403"
(Not "/ErrorPage" or "/Error/403.cshtml")
```

---

## ?? TEST RESULTS TEMPLATE

Copy and paste this template to record your tests:

```
ERROR HANDLING TEST RESULTS
===========================

Test Date: _________
Tester: _________

Test 1: 404 Not Found
- Navigate to /InvalidPage
- Error page displays: YES / NO
- Correct title shown: YES / NO
- Audit log entry exists: YES / NO
- Path in audit log: _________

Test 2: 403 Forbidden
- Login as non-admin
- Navigate to /AdminTest
- Error page displays: YES / NO
- Correct title shown: YES / NO
- Audit log entry exists: YES / NO
- UserId in audit log: _________

Test 3: 500 Internal Error
- Trigger exception
- Error page displays: YES / NO
- RequestId shown: YES / NO
- Audit log entry exists: YES / NO

Test 4: 429 Rate Limited
- Make 6+ rapid login attempts
- Error page displays: YES / NO
- Correct message shown: YES / NO
- Audit log entry exists: YES / NO

Status: PASS / FAIL
Comments: _____________________________
```

---

## ? EXPECTED AUDIT LOG ENTRIES

After completing all tests, your /AuditLogs page should show:

```
1. Page Not Found (Status: 404, Path: /InvalidPage)
2. Access Denied (Forbidden) (Status: 403, Path: /AdminTest)
3. Internal Server Error (Status: 500, Path: /Index)
4. Rate Limit Exceeded (Status: 429, Path: /Login)
5. ... plus any other errors triggered during testing
```

All entries should have:
- Timestamp (matches when you triggered the error)
- IP Address (your computer)
- Path (the URL you accessed)
- Status Code (in Details column)

---

## ?? DEMO WALKTHROUGH

If demonstrating to tutor:

**1. Show 404 Error** (1 min)
```
"Let me show you the professional error handling..."
Navigate to /InvalidPage
"See the clean error page - not a scary error code"
```

**2. Show 403 Error** (1 min)
```
"Different errors get customized messages..."
Login, navigate to /AdminTest
"This is a 403 - Access Denied, different from 404"
```

**3. Show Audit Logs** (2 min)
```
"Every error is logged for security..."
Go to /AuditLogs
"Here are the error entries:
- Action: what error occurred
- Path: what they tried to access
- Timestamp: when it happened
- IP: where they came from

This is the audit trail for compliance"
```

---

**Testing Guide Complete. Ready to verify your error handling!** ?

