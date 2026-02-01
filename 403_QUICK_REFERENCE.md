# ?? 403 ERROR HANDLING - QUICK REFERENCE CARD

## ? TL;DR - The Fix in 30 Seconds

**Problem**: 403 errors weren't showing custom error page
**Solution**: 3 simple changes

```csharp
// 1. Change AccessDeniedPath (Program.cs)
options.AccessDeniedPath = "/ErrorPage?statusCode=403";

// 2. Move Middleware (Program.cs)
// Put UseStatusCodePagesWithReExecute AFTER UseAuthorization

// 3. Audit Log (Already done)
// ErrorPageModel logs 403 as "Access Denied (Forbidden)"
```

---

## ?? TEST IT (2 minutes)

```
1. dotnet run
2. Login as non-admin user
3. Go to: https://localhost:7257/AdminTest
4. See: Professional 403 page
5. Check: /AuditLogs for entry
```

---

## ?? WHAT YOU GET

| Item | Status | Details |
|------|--------|---------|
| Error Page | ? | Professional 403 page |
| Audit Log | ? | "Access Denied" entry |
| Build | ? | 0 errors, 0 warnings |
| Marks | ? | 15% achievable |

---

## ?? DEMO (3 minutes)

```
1. Login ? /AdminTest ? See error page
2. /AuditLogs ? Show entry
3. Explain: 5% error handling + 10% logging = 15% total
```

---

## ?? KEY FILES

```
Program.cs
?? Line 53: options.AccessDeniedPath
?? Line 133: UseStatusCodePagesWithReExecute

ErrorPage.cshtml.cs
?? Line 50: case 403 mapping
?? LogErrorToAuditAsync()

AdminTest.cshtml.cs (NEW)
?? [Authorize(Roles = "Admin")]
```

---

## ? CHECKLIST

- [x] AccessDeniedPath = "/ErrorPage?statusCode=403"
- [x] UseStatusCodePages AFTER UseAuthorization()
- [x] 403 logged as "Access Denied (Forbidden)"
- [x] AdminTest page created
- [x] Build successful
- [x] Ready for demo

---

## ?? STATUS: READY FOR DEMO ?

Build: ? | Config: ? | Logging: ? | Demo: ?

