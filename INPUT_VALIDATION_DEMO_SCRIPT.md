# ?? INPUT VALIDATION & SECURITY - DEMO SCRIPT & QUICK REFERENCE

## ? 10-MINUTE TUTOR DEMO

### Setup
```
Application running: https://localhost:7257
Demo pages:
  - /Register (Register form)
  - /Login (Login form)
  - /AuditLogs (Audit trail)
```

### Demo 1: Client-Side Validation (2 min)
```
Action: Go to Register page

Step 1: Try to submit empty form
- Click "Register" without filling anything
- Result: Instant red error messages appear
  ? "Full Name is required"
  ? "Gender is required"
  ? "Mobile Number is required"
  ? "Email is required"
  ? "Password is required"
- Point out: Errors appear instantly, NO page reload

Step 2: Enter weak password
- Type in Password: "abc"
- Result: Red "Weak" strength meter appears
- Missing: At least 12 characters
  (Shows list of what's missing)

Step 3: Enter strong password
- Type: "SecurePass123!@#"
- Result: Green "Strong" strength meter
- All requirements met
```

### Demo 2: Invalid Input Formats (2 min)
```
Action: Fill Register form with INVALID data

Step 1: Invalid Email
- Enter Email: "notanemail"
- Error message appears: "Please enter a valid email address"
- Browser prevents submission

Step 2: Invalid Phone Number
- Enter Mobile: "12345" (wrong format)
- Error: "Mobile number must be 8 digits starting with 8 or 9"
- Field rejects input

Step 3: Invalid Credit Card
- Enter Card: "1234" (only 4 digits)
- Error: "Credit Card Number must be exactly 16 digits"
- Auto-formats to digits only

Step 4: Photo Not JPG
- Upload: any.png or .pdf file
- Error: "Only .JPG files are allowed"
- JavaScript blocks the upload
```

### Demo 3: Server-Side Validation (2 min)
```
Action: Demonstrate server-side validation still works

Step 1: Disable JavaScript
- Open DevTools (F12)
- Go to Settings ? Disable JavaScript
- Page refreshes

Step 2: Fill form and submit
- Even with JavaScript disabled
- Server still validates all fields
- Shows the same error messages
- Point out: "This proves validation happens on SERVER, not just client"

Step 3: Try XSS payload in "About Me"
- Enable JavaScript again
- Full form with:
  - About Me: <script>alert('XSS')</script>
- Submit
- Look at profile page
- Script renders as PLAIN TEXT, doesn't execute
- Point out: "Even if JavaScript executes malicious code on client,
  it doesn't persist because we HTML-encode it server-side"
```

### Demo 4: Duplicate Email Prevention (1 min)
```
Action: Try to register with existing email

Step 1: Register new user
- Email: newuser@test.com
- Complete registration successfully

Step 2: Try to register again with same email
- Email: newuser@test.com
- Submit form
- Error: "This email address is already registered"
- Point out: Database check prevents duplicates
```

### Demo 5: Account Lockout & Audit Logging (2 min)
```
Action: Demonstrate failed login attempts and audit trail

Step 1: Go to Login page
- Try to login with wrong password 3 times
- Account locks after 3 failures
- Error: "Account locked due to multiple failed attempts"
- Can't login for 15 minutes

Step 2: Check Audit Logs
- Go to /AuditLogs (login as different user)
- Find the locked account
- See entries:
  ? "Login Failed" (3 entries)
  ? "Account Locked" (1 entry)
- Each entry shows:
  ? Timestamp (when attempt occurred)
  ? IP Address (where from)
  ? Failed attempt count
- Point out: "Every security event is logged for monitoring"

Step 3: Wait 15 minutes (or show code)
- Account automatically unlocks
- Can login again
- Explain: "Auto-recovery prevents permanent lockouts"
```

### Demo 6: CSRF Protection (1 min)
```
Action: Show CSRF token in forms

Step 1: Go to any form (Login/Register)
- Open browser DevTools (F12)
- Go to Elements/Inspector tab
- Right-click on form ? Inspect

Step 2: Find the CSRF token
- Look for: <input name="__RequestVerificationToken"
- Value: "CfDJ8KzWn..." (long encrypted token)
- Point out: "This hidden field is included in EVERY form"

Step 3: Explain protection
- Every form submission validates this token
- Token is tied to user's session
- Prevents cross-site forgery attacks
- Anyone can't submit form from external website
```

---

## ?? VALIDATION RULES CHECKLIST

### Register Form Validation Rules

| Field | Rules | Error Message |
|-------|-------|---------------|
| Full Name | Required, Max 100 chars | "Full Name is required" |
| Gender | Required, Select from list | "Gender is required" |
| Mobile No | Required, 8 digits, starts 8/9 | "must be 8 digits starting with 8 or 9" |
| Delivery Address | Required, Max 500 chars | "Delivery Address is required" |
| Email | Required, Valid email format | "Please enter a valid email address" |
| Password | Required, 12+ chars, Upper, Lower, Digit, Special | "does not meet complexity requirements" |
| Confirm Password | Required, Must match Password | "Password and confirmation do not match" |
| Credit Card | Required, Exactly 16 digits | "must be exactly 16 digits" |
| About Me | Required, Special chars allowed | "About Me is required" |
| Photo | Required, .JPG only, Max 2MB | "Only .JPG files are allowed" |

### Login Form Validation Rules

| Field | Rules | Error Message |
|-------|-------|---------------|
| Email | Required, Valid email | "Please enter a valid email address" |
| Password | Required | "Password is required" |
| reCAPTCHA | Required | "Security verification failed" |

---

## ?? SECURITY FEATURES TO HIGHLIGHT

### 1. Password Requirements
```
? Minimum 12 characters (not 8!)
? Must contain uppercase letter
? Must contain lowercase letter
? Must contain number
? Must contain special character (!@#$%^&*)
? Password strength meter (real-time feedback)
? Prevent password reuse (last 2 passwords)
```

### 2. Account Lockout
```
? 3 failed login attempts ? Account locked
? Automatic 15-minute lockout
? Auto-recovery after lockout expires
? Failed attempts logged
? Email address not revealed (prevents enumeration)
```

### 3. Data Encryption
```
? Credit card numbers encrypted (AES-256)
? Passwords hashed (Identity PasswordHasher)
? Database encryption-ready
? HTTPS enforced
```

### 4. Audit Trail
```
? All login attempts logged
? All registrations logged
? All password changes logged
? All permission denials logged
? Failed validations logged
? Timestamps recorded
? IP addresses recorded
? User IDs recorded
? reCAPTCHA scores recorded
```

---

## ?? WHAT TO SAY (Tutor Script)

### Opening Statement
```
"I've implemented comprehensive input validation and security
across the entire application. Every form has both client-side 
validation for immediate feedback, and server-side validation 
to prevent tampering. All database queries use Entity Framework 
Core LINQ which is immune to SQL injection. User inputs are 
HTML-encoded to prevent XSS attacks. All forms use anti-forgery 
tokens to prevent CSRF attacks. And every security event—failed 
logins, validation failures, permission denials—is logged to 
the audit trail with complete context for compliance."
```

### When showing validation
```
"Notice how the error messages appear instantly without a page 
reload. This is client-side validation for good UX. But on the 
server, I also validate everything. Even if someone disables 
JavaScript and crafts a malicious request, the server still 
validates and rejects it."
```

### When showing audit logs
```
"Every security event is logged here. You can see failed login 
attempts, account lockouts, password changes, and permission 
denials. Each entry includes a timestamp, IP address, and user 
ID. This creates a complete audit trail for compliance and 
security monitoring."
```

### When showing encryption
```
"Sensitive data like credit card numbers are encrypted using 
AES-256 encryption. Passwords are hashed using the ASP.NET Core 
Identity PasswordHasher. Even if someone gained database access, 
they can't read the credit card numbers or password hashes."
```

---

## ? BEFORE-TUTOR CHECKLIST

- [ ] Application is running (https://localhost:7257)
- [ ] Can navigate to /Register page
- [ ] Can navigate to /Login page
- [ ] Can navigate to /AuditLogs page
- [ ] Have test account created for login demo
- [ ] Have invalid test data ready (XSS payload, etc.)
- [ ] Browser DevTools ready (F12)
- [ ] Know how to disable JavaScript in browser
- [ ] Have demo script memorized (or printed)
- [ ] Practice demo (5 minutes)

---

## ?? RUBRIC MARKS

**Input Validation & Security** = 20 marks

- [x] **Comprehensive Validation** (5 marks) - Server + Client
- [x] **SQL Injection Prevention** (5 marks) - LINQ only
- [x] **XSS Prevention** (5 marks) - HtmlEncoder + Razor
- [x] **CSRF Prevention** (3 marks) - Anti-forgery tokens
- [x] **Audit Logging** (2 marks) - All events logged

---

**Ready to demonstrate your security implementation!** ??

