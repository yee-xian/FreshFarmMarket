# ?? Password Reset Feature - Demo Script

## ?? What to Show (5-7 minutes total)

---

## **Segment 1: Forgot Password Flow (1 minute)**

### What You'll Say:
```
"Let me demonstrate the password reset feature. First, I'll show the 
'Forgot Password' flow that users access when they forget their credentials."
```

### Step-by-Step:

1. **Navigate to Login Page**
   ```
 URL: https://localhost:7257/Login
   Show: Fresh Farm Market login form with email/password fields
   ```

2. **Click "Forgot Password" Link**
   ```
   Location: Below login button (gray link)
   Result: Navigate to /ForgotPassword page
   ```

3. **Enter Email Address**
   ```
   Field: "Email Address"
   Enter: user@example.com
   Show: Email validation (must be valid format)
   ```

4. **Click "Send Reset Link" Button**
   ```
   Button: Green button with send icon
   Result: Success message appears
   Message: "If an account exists with that email, a password reset link has been sent."
   Security: Note the email enumeration prevention (same message for any email)
   ```

5. **Show Console Log (Explanation)**
   ```
   Open: Browser DevTools ? Console tab
   OR: Visual Studio Output window
   Show: "Email sent (Demo Mode)" log entry
   Explain: In production, would send real email via SMTP
   ```

### What to Say:
```
"Notice how we show the same message whether the email exists or not. 
This prevents 'email enumeration attacks' where someone could list 
valid email addresses in the database. For security, we don't confirm 
whether an email is registered."
```

---

## **Segment 2: Password Strength & Requirements (1.5 minutes)**

### What You'll Say:
```
"Now let's look at the reset password form. The system enforces 
strong password requirements - 12 characters minimum with upper, 
lower, number, and special character."
```

### Step-by-Step:

1. **Copy Reset Link**
   ```
   From: Console log or email (in demo mode shows in console)
   Format: https://localhost:7257/ResetPassword?userId=XX&token=YY
   Copy: The entire URL
   ```

2. **Open Reset Link**
   ```
   Paste: URL into browser address bar
   Result: /ResetPassword page loads
   Check: Form displayed with "New Password" and "Confirm Password" fields
   ```

3. **Test Password Strength Meter (Weak)**
   ```
   Click: New Password field
   Enter: "password"
   Show: Red progress bar labeled "Weak"
   Point: "Missing uppercase, number, special character"
   ```

4. **Enter Stronger Password**
   ```
   Clear: Previous password
   Enter: "Pass123"
   Show: Yellow progress bar labeled "Medium"
   Explain: "Has uppercase and number, but only 7 characters"
 ```

5. **Enter Valid Strong Password**
   ```
   Clear: Previous password
   Enter: "NewPass123!"
 Show: Green progress bar labeled "Strong"
 All checks: ? 12+ characters, ? Upper, ? Lower, ? Number, ? Special
   Highlight: Real-time validation helps users
   ```

6. **Show Password Visibility Toggle**
   ```
   Click: Eye icon in password field
   Result: Password changes from dots to visible text
   Say: "Users can toggle visibility for security comfort"
   ```

### What to Say:
```
"The password complexity is enforced both on the client side (real-time 
feedback) and on the server side (validation on submit). This dual-layer 
approach prevents both poor user experience and security bypasses."
```

---

## **Segment 3: Confirm Password Match (30 seconds)**

### Step-by-Step:

1. **Enter Matching Confirmation**
   ```
New Password: NewPass123!
   Confirm Password: NewPass123!
   Show: Both fields highlighted (match)
   ```

2. **Test Mismatch (Optional)**
   ```
   Confirm Password: NewPass123 (without !)
   Show: Error message "Passwords do not match"
   Fix: Make them match again
   ```

### What to Say:
```
"The confirmation field ensures users don't make typos in their password."
```

---

## **Segment 4: Password History Protection (1 minute)**

### What You'll Say:
```
"One important security feature is password history. The system tracks 
the last 2 passwords and prevents users from reusing them."
```

### Step-by-Step:

1. **Complete Password Reset**
   ```
   New Password: NewPass123!
   Confirm Password: NewPass123!
   Click: "Reset Password" button
   ```

2. **Show Success Message**
   ```
   Message: "Your password has been reset successfully. You can now log in 
         with your new password."
 Button: "Go to Login" link
   ```

3. **Check Audit Log (Important!)**
   ```
   Click: "Go to Login" button ? redirects to Login page
   Login: With new password (NewPass123!)
   Click: "Activity Logs" (after login)
 URL: /AuditLogs
   ```

4. **Show Audit Entries**
   ```
   Entry 1: "Password Reset Requested" - timestamp, email
   Entry 2: "Password Reset" - successful reset logged
   Entry 3: "Login Success" - with new password
   Point: Complete security trail for compliance
   ```

### What to Say:
```
"The system logs every password reset attempt. If a user later claims 
their account was compromised, we have a complete audit trail. The password 
history also prevents 'password cycling' where users just rotate through 
the same passwords."
```

---

## **Segment 5: Live Password Reset Demo (1-2 minutes)**

### Alternative (if time permits):

1. **Show Multiple Reset Scenarios**
   ```
   Scenario 1: Correct password requirements ? Success
   Scenario 2: Trying to reuse old password ? Error
   Scenario 3: Token expired ? "Please request a new link"
   Scenario 4: Invalid token ? "Invalid password reset link"
   ```

2. **Show Database Records**
   ```
   Open: SQL Server Management Studio or Database Explorer
   Table: PasswordHistory
 Show: Hash of each password attempt
   Explain: Passwords are never stored in plain text
   ```

3. **Show Token Expiry (Optional)**
   ```
   Wait: 1 hour (simulated - you can modify token expiry for demo)
   Try: Opening old reset link
   Result: "This password reset link has expired..."
   Point: Security - tokens don't last forever
   ```

---

## **Segment 6: Integration Demo (1 minute)**

### What You'll Say:
```
"Let me show how this integrates with the rest of the security system."
```

### Step-by-Step:

1. **Show Configuration**
   ```
 File: appsettings.json
   Show: Token expiry settings (can be configured)
   Show: Password policy (12 chars, complexity)
   Show: Email SMTP settings (production mode)
 ```

2. **Show Code Comments**
   ```
   File: ResetPasswordModel.cs
   Highlight: Code comments explaining:
      - Token validation
- Password history checking
      - Audit logging
      - Error handling
   ```

3. **Show Error Messages**
   ```
   Invalid token: "Invalid password reset link."
   Expired token: "This password reset link has expired..."
   Weak password: "Password must contain uppercase, lowercase, number..."
   Password reused: "You cannot reuse any of your last 2 passwords."
   ```

---

## **Key Points to Emphasize**

### ?? Security
- "Tokens expire after 1 hour"
- "Passwords hashed with bcrypt"
- "Email enumeration prevented"
- "Password history prevents reuse"

### ?? Compliance
- "Complete audit trail"
- "Timestamps on every action"
- "IP address tracked"
- "User email logged"

### ?? User Experience
- "Real-time strength meter"
- "Password visibility toggle"
- "Clear error messages"
- "Professional email template"

### ??? Defense in Depth
- "Client-side validation"
- "Server-side validation"
- "Token validation"
- "Password complexity enforcement"

---

## **Talking Points**

### If Asked: "Why require 12 characters?"
```
"Passwords with 12+ characters are exponentially harder to crack. 
Combined with uppercase, lowercase, numbers, and special characters, 
an attacker would need to try 94^12 combinations (far too many)."
```

### If Asked: "Why track password history?"
```
"Users often rotate through passwords thinking they're being clever. 
'Password123', 'Password456', etc. Password history prevents this 
vulnerability by blocking the last 2 passwords."
```

### If Asked: "Why 1-hour token expiry?"
```
"1 hour balances security and usability. If tokens lasted too long, 
they could be compromised. If too short, users wouldn't have time to 
use them. 1 hour is the industry standard."
```

### If Asked: "How does the email work?"
```
"In development, emails are logged to the console (you saw it earlier). 
In production, it uses SMTP (Simple Mail Transfer Protocol) to send real 
emails via services like Gmail, SendGrid, or your organization's mail server."
```

---

## **Quick Reference - Test Credentials**

If you need to reset a password during demo:

```
Email: user@example.com    (must exist in database)
OR
Email: test@freshfarm.com   (any valid email format)

Test Passwords:
? weak        ? Too short, missing requirements
? Pass123      ? Missing special character
? PASSWORD123!      ? Missing lowercase
? password123!  ? Missing uppercase
? NewPass123!       ? Valid (12+ chars, upper, lower, digit, special)
? SecurePass456@    ? Valid alternative
```

---

## **Timing Guide**

| Segment | Time | Notes |
|---------|------|-------|
| **Setup** | 30 sec | Navigate to login, explain what you'll show |
| **Forgot Password Flow** | 1 min | Email request, show console |
| **Password Strength** | 1.5 min | Demo weak/medium/strong passwords |
| **Confirmation Match** | 30 sec | Quick demo of matching validation |
| **Password History** | 1 min | Reset and audit log |
| **Integration** | 1 min | Code, config, error messages |
| **Questions** | Open | Answer any questions |
| **TOTAL** | **5-7 min** | Perfect for rubric demonstration |

---

## **Contingency Plans**

### If Email Service Fails
```
"The system is configured for development mode. Normally, this would 
send a real email. Let me show you the email template instead."
? Show: EmailService.cs SendPasswordResetEmailAsync method
```

### If Database Query Fails
```
"This might be a database connection issue. Let me show you the 
password history structure instead."
? Show: Model/PasswordHistory.cs file
```

### If Token Expires During Demo
```
"Perfect timing to show the token expiry feature. Let me request a new link."
? Go back to ForgotPassword, request new link
```

### If Password Doesn't Meet Requirements
```
"Great, let me show what's missing and fix it."
? Adjust password based on strength meter feedback
```

---

## **Final Talking Points**

```
"This password reset system demonstrates several key security principles:

1. **Complexity**: Strong passwords (12+ characters, 4 character types)
2. **History**: Cannot reuse recent passwords
3. **Verification**: Token-based email confirmation
4. **Auditing**: Every action logged for compliance
5. **Timing**: Tokens expire after 1 hour
6. **Transparency**: Clear error messages
7. **Usability**: Real-time feedback and toggles

Together, these features create a secure password reset system that 
protects both the user and the organization."
```

---

**You're ready to demo! Good luck!** ??

