# Apply reCAPTCHA Score Database Migration

## Quick Steps

### Step 1: Stop the Application
Press `Ctrl+C` in the terminal running your app.

### Step 2: Create Migration
```bash
cd "C:\Users\Yee Xian\Downloads\WebApplication1\WebApplication1"
dotnet ef migrations add AddRecaptchaScoreToAuditLog
```

### Step 3: Apply Migration
```bash
dotnet ef database update
```

### Step 4: Restart Application
```bash
dotnet run
```

---

## What the Migration Does

Adds a new column to the `AuditLogs` table:

```sql
-- SQL equivalent
ALTER TABLE AuditLogs
ADD RecaptchaScore FLOAT NULL;
```

**Details**:
- Column name: `RecaptchaScore`
- Type: `FLOAT (nullable)`
- Default: `NULL`
- Stores values: 0.0 to 1.0
- No existing data affected

---

## Verify Migration Success

After running `dotnet ef database update`, you should see:

```
Build started...
Build succeeded.
Applying migration '20250131XXXXXX_AddRecaptchaScoreToAuditLog'.
Done.
```

---

## Test the Feature

1. Open your app: https://localhost:7257/Login
2. Login with valid credentials
3. Navigate to Activity Logs
4. Check the "reCAPTCHA Score" column
5. You should see scores like:
   - `0.95` (green badge)
- `0.87` (green badge)
   - `0.32` (red badge)

---

## Troubleshooting

### Error: "Build failed"
- Make sure app is stopped (Ctrl+C)
- Run: `dotnet build` to check for compilation errors

### Error: "DB context out of sync"
- Delete `Migrations/XXXX_*.cs` files added
- Run: `dotnet ef migrations remove`
- Try again

### Score column not showing
- Clear browser cache (Ctrl+Shift+Delete)
- Reload page (F5)
- Check database updated with `SELECT * FROM AuditLogs`

---

## What You'll See

After migration, Activity Logs page will show:

```
Date & Time          | Action       | Details           | reCAPTCHA Score | IP Address
2025-01-31 14:30:45 | Login Success   | User logged in... | 0.95 [green]   | 192.168.1.1
2025-01-31 14:28:12 | Login Failed    | Invalid pwd...  | 0.32 [red]     | 192.168.1.2
2025-01-31 14:15:30 | Reg Success     | User registered..| 0.87 [green]   | 192.168.1.3
```

**Color Legend**:
- ?? Green: 0.9+ (Human)
- ?? Blue: 0.7+ (Likely human)
- ?? Yellow: 0.5+ (Suspicious)
- ?? Red: <0.5 (Bot)
- Gray: No score

---

## Done! ?

Your application now tracks and displays reCAPTCHA scores in activity logs!
