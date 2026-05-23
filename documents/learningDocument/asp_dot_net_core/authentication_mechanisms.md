# Authentication Mechanisms: Cookies, Token, Session & Hybrid

---

## Part 1 — HTTP is Stateless

Before understanding any authentication mechanism, you need to understand the root problem.

**HTTP is stateless** — each request is completely independent. The server has no memory of your previous requests.

```
Request 1: "I am John Doe, show me my orders"
Server processes... returns response... immediately forgets.

Request 2: "Show me my profile"
Server: "Who are you?"  ← remembers nothing from request 1
```

Every authentication mechanism is a **solution to this statelessness problem** — how can the server "recognize" a user on each request without requiring them to log in again?

---

## Part 2 — Cookies

### 2.1 What is a Cookie?

A **cookie** is a small piece of data the server sends to the browser. The browser **automatically stores it and automatically includes it** in every subsequent request to the same domain — no extra code needed from the developer.

```
Server → Response:
Set-Cookie: user_id=5; Path=/; HttpOnly; Secure; SameSite=Strict

Browser saves the cookie.

Browser → Next Request:
Cookie: user_id=5   ← sent automatically
```

### 2.2 Important Cookie Attributes

```
Set-Cookie: session_id=abc123
  ; Path=/              → send this cookie for all paths on the domain
  ; Domain=myapp.com    → only send to this domain
  ; Expires=Thu, 01 Jan 2026 00:00:00 GMT  → specific expiry date
  ; Max-Age=3600        → expires after 3600 seconds
  ; HttpOnly            → JS cannot read it → blocks XSS
  ; Secure              → only sent over HTTPS
  ; SameSite=Strict     → not sent on cross-site requests → blocks CSRF
```

| Attribute | Security Effect |
|-----------|----------------|
| `HttpOnly` | JS cannot read the cookie → blocks XSS-based cookie theft |
| `Secure` | Only sent over HTTPS → blocks network sniffing |
| `SameSite=Strict` | Not sent cross-site → blocks CSRF |
| `SameSite=Lax` | Allows cross-site navigation but blocks cross-site POST |

### 2.3 Cookies Are Not Authentication by Themselves

A cookie is only a **transport mechanism** — a bag for carrying data between the browser and server. What's inside the bag determines the authentication mechanism.

```
Cookie contains Session ID   → Session-based Auth
Cookie contains JWT Token    → Token-based Auth via Cookie
Cookie contains User ID      → very dangerous, never do this
```

---

## Part 3 — Session-based Authentication

### 3.1 How It Works

Session-based authentication is a **server-side stateful** mechanism. The server keeps a "notebook" recording who is logged in.

```
┌─────────────────────────────────────────────────────┐
│                    SESSION STORE                     │
│  "abc123" → { userId: 5, name: "John Doe",          │
│               role: "Customer", loginAt: "10:00" }  │
│  "def456" → { userId: 8, name: "Jane Smith", ... }  │
└─────────────────────────────────────────────────────┘
```

Login flow:

```
1. User sends: POST /login { email, password }
                │
2. Server validates credentials
   Creates session:  sessionId = "abc123"
   Saves to store:   sessions["abc123"] = { userId: 5, role: "Customer" }
                │
3. Server responds:
   Set-Cookie: sessionId=abc123; HttpOnly; Secure
                │
4. Browser saves the cookie.

Subsequent requests:
5. Browser automatically sends:  Cookie: sessionId=abc123
                │
6. Server reads sessionId from cookie
   Looks up store:  sessions["abc123"] → { userId: 5, role: "Customer" }
   → Knows who the user is without asking again
```

### 3.2 Session Store — Where to Store Sessions?

```
In-Memory (default)
  └── Fastest, but lost on server restart
      Cannot be used across multiple servers

Database (SQL/NoSQL)
  └── Durable, scalable
      Requires a DB query on every request → slower

Redis (most common for production)
  └── Durable + fast (in-memory DB)
      Scales well across multiple servers
      Automatic TTL-based session expiry
```

### 3.3 Implementing Sessions in ASP.NET Core

```csharp
// Program.cs
builder.Services.AddDistributedMemoryCache();  // or AddStackExchangeRedisCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout         = TimeSpan.FromMinutes(30);  // expires after 30min idle
    options.Cookie.HttpOnly     = true;
    options.Cookie.IsEssential  = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// In the pipeline
app.UseSession();   // must come before MapControllers

// In a Controller
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    var user = await _authService.ValidateAsync(dto.Email, dto.Password);
    if (user == null)
        return Unauthorized(new { message = "Invalid email or password" });

    // Store user info in session
    HttpContext.Session.SetInt32("UserId", user.Id);
    HttpContext.Session.SetString("UserName", user.Name);
    HttpContext.Session.SetString("Role", user.Role);

    return Ok(new { message = "Login successful" });
}

[HttpGet("me")]
public IActionResult GetMe()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return Unauthorized();

    return Ok(new
    {
        id   = userId,
        name = HttpContext.Session.GetString("UserName"),
        role = HttpContext.Session.GetString("Role")
    });
}

[HttpPost("logout")]
public IActionResult Logout()
{
    HttpContext.Session.Clear();   // clear the entire session
    return Ok(new { message = "Logged out successfully" });
}
```

### 3.4 Pros and Cons

**Pros:**
- Sessions can be revoked immediately — just delete from the store
- Sensitive data stays on the server, never exposed to the client
- Easy to implement for traditional web apps

**Cons:**
- **Stateful** — server must maintain state → difficult to scale horizontally
- Multiple servers must share the same session store (Redis)
- Not suitable for mobile apps or third-party APIs
- Vulnerable to **CSRF attacks** if SameSite or CSRF tokens are not used

---

## Part 4 — Token-based Authentication

### 4.1 How It Works

Token-based authentication is **stateless** — the server stores nothing. All information lives in the token held by the client.

```
1. User sends: POST /login { email, password }
                │
2. Server validates credentials
   Creates JWT:  header.payload.signature
   Stores NOTHING in DB or memory
                │
3. Server responds: { "token": "eyJ..." }
                │
4. Client stores the token (localStorage / memory / cookie)

Subsequent requests:
5. Client sends:  Authorization: Bearer eyJ...
                │
6. Server verifies the token's signature using the secret key
   Decodes payload → knows userId, role
   No DB or memory lookup required
```

### 4.2 JWT in Depth — Signing vs Encryption

Two concepts that are often confused:

**Signing (commonly used):**
```
Payload is encoded (base64) → anyone can read it
Signature ensures the payload has not been tampered with

→ Use when: data is not sensitive (userId, role)
→ Algorithms: HS256 (HMAC), RS256 (RSA)
```

**Encryption (less common):**
```
Payload is encrypted → unreadable without the key
→ Use when: payload contains sensitive information
→ Algorithms: RSA-OAEP, AES
```

**Important:** A JWT is by default only **signed, not encrypted**. The payload is fully readable by decoding base64. Never put passwords or credit card details in a JWT payload.

```
eyJhbGciOiJIUzI1NiJ9         → decode → {"alg":"HS256"}
.eyJ1c2VySWQiOjV9            → decode → {"userId":5}         ← anyone can read this
.SflKxwRJSMeKKF2QT4fwpMeJ   → NOT decodable without the secret key
```

### 4.3 Token Storage — Where to Store the Token?

This is a critical security decision:

```
localStorage / sessionStorage
  ├── Easy to use, persists across tabs
  ├── Readable by JS → vulnerable to XSS token theft
  └── NOT recommended for sensitive tokens

Memory (JS variable)
  ├── Readable by JS but lost on page refresh
  ├── Safer than localStorage
  └── Requires a Refresh Token to restore the Access Token

HttpOnly Cookie
  ├── JS CANNOT read it → blocks XSS
  ├── Automatically sent with requests → convenient
  ├── Requires SameSite + CSRF protection
  └── Most recommended option for web apps
```

### 4.4 Pros and Cons

**Pros:**
- Stateless → easy to scale, no shared store needed
- Suitable for mobile apps, SPAs, and third-party APIs
- Works across domains easily (unlike cookies)

**Cons:**
- Tokens cannot be revoked immediately — must wait for expiry
- Token is larger than a Session ID → slightly more bandwidth
- Payload is readable → do not store sensitive data in it

---

## Part 5 — Session vs Token Comparison

```
                    SESSION                 TOKEN (JWT)
                ─────────────────       ─────────────────
State storage   Server (store)           Client (token)
Stateless        ❌ Stateful              ✅ Stateless
Horizontal scale Requires shared store    Easy
Immediate revoke ✅ Delete from store     ❌ Must wait for expiry
Mobile app       ❌ Difficult             ✅ Easy
Cross-domain     ❌ Cookie restrictions   ✅ Header has no restrictions
Data security    ✅ Data stays on server  ⚠️ Payload is readable
CSRF risk        ⚠️ Yes if using cookie   ✅ No if using header
XSS risk         ✅ HttpOnly cookie       ⚠️ If stored in localStorage
```

---

## Part 6 — Hybrid: Combining Token + Session

In practice, many systems **combine both** to leverage the strengths of each mechanism.

### 6.1 Pattern: JWT in an HttpOnly Cookie

The most common pattern for web apps today:

```
Statelessness of JWT   +   Security of HttpOnly Cookie
```

```
1. Login → server creates JWT
2. Server stores JWT in an HttpOnly Cookie (not returned in body)
3. Browser automatically sends the cookie with every request
4. Server reads JWT from cookie and verifies the signature

→ JS cannot read the token (blocks XSS)
→ Server stores no state (stateless)
→ CSRF protection is still needed since cookies are used
```

```csharp
// Login — store JWT in a cookie instead of returning it in the body
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    var user = await _authService.ValidateAsync(dto.Email, dto.Password);
    if (user == null)
        return Unauthorized();

    var token = _authService.GenerateToken(user);

    // Store JWT in an HttpOnly Cookie
    Response.Cookies.Append("access_token", token, new CookieOptions
    {
        HttpOnly = true,                           // JS cannot read it
        Secure   = true,                           // HTTPS only
        SameSite = SameSiteMode.Strict,            // blocks CSRF
        Expires  = DateTimeOffset.UtcNow
                       .AddMinutes(60)
    });

    // Do not return the token in the response body
    return Ok(new { message = "Login successful", name = user.Name });
}

// Configure JWT to read from Cookie instead of Authorization header
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer   = true,
            ValidateAudience = true,
            ValidIssuer      = "MyApp",
            ValidAudience    = "MyUsers",
            ClockSkew        = TimeSpan.Zero
        };

        // Read token from cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });
```

### 6.2 Pattern: Access Token + Refresh Token in Cookies

The most complete pattern, balancing security and user experience:

```
access_token cookie:
  HttpOnly, Secure, SameSite=Strict
  Expires: 15 minutes
  → Used to authenticate each request

refresh_token cookie:
  HttpOnly, Secure, SameSite=Strict
  Path=/api/auth/refresh          ← only sent to the refresh endpoint
  Expires: 7 days
  → Used to obtain a new access_token when it expires
```

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    var user = await _authService.ValidateAsync(dto.Email, dto.Password);
    if (user == null) return Unauthorized();

    var accessToken  = _authService.GenerateAccessToken(user);   // 15 minutes
    var refreshToken = _authService.GenerateRefreshToken();       // random string

    // Persist refresh token to DB
    await _tokenRepo.SaveAsync(new RefreshToken
    {
        Token     = refreshToken,
        UserId    = user.Id,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });

    // Write both tokens to HttpOnly Cookies
    AppendTokenCookies(accessToken, refreshToken);

    return Ok(new { message = "Login successful" });
}

[HttpPost("refresh")]
public async Task<IActionResult> Refresh()
{
    // Read refresh token from cookie
    var refreshToken = Request.Cookies["refresh_token"];
    if (string.IsNullOrEmpty(refreshToken))
        return Unauthorized();

    // Validate against DB
    var stored = await _tokenRepo.FindAsync(refreshToken);
    if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
    {
        // Invalid token — clear cookies and require re-login
        DeleteTokenCookies();
        return Unauthorized(new { message = "Session expired, please log in again" });
    }

    var user = await _userRepo.GetByIdAsync(stored.UserId);

    // Issue new tokens — Refresh Token Rotation
    var newAccessToken  = _authService.GenerateAccessToken(user);
    var newRefreshToken = _authService.GenerateRefreshToken();

    // Delete old token, save new one (Rotation — prevents reuse)
    await _tokenRepo.DeleteAsync(stored.Id);
    await _tokenRepo.SaveAsync(new RefreshToken
    {
        Token     = newRefreshToken,
        UserId    = user.Id,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });

    AppendTokenCookies(newAccessToken, newRefreshToken);
    return Ok(new { message = "Token refreshed successfully" });
}

[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var refreshToken = Request.Cookies["refresh_token"];
    if (!string.IsNullOrEmpty(refreshToken))
        await _tokenRepo.DeleteByTokenAsync(refreshToken);  // revoke immediately

    DeleteTokenCookies();
    return Ok(new { message = "Logged out successfully" });
}

// Helper methods
private void AppendTokenCookies(string accessToken, string refreshToken)
{
    Response.Cookies.Append("access_token", accessToken, new CookieOptions
    {
        HttpOnly = true, Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires  = DateTimeOffset.UtcNow.AddMinutes(15)
    });

    Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
    {
        HttpOnly = true, Secure = true,
        SameSite = SameSiteMode.Strict,
        Path     = "/api/auth/refresh",   // only sent to this endpoint
        Expires  = DateTimeOffset.UtcNow.AddDays(7)
    });
}

private void DeleteTokenCookies()
{
    Response.Cookies.Delete("access_token");
    Response.Cookies.Delete("refresh_token",
        new CookieOptions { Path = "/api/auth/refresh" });
}
```

### 6.3 Pattern: Session for Web + JWT for Mobile/API

For large systems serving both web browsers and mobile apps:

```
                    ┌─────────────────────┐
                    │       Server        │
                    │                     │
Browser ──Cookie──▶│  Session-based Auth │
                    │  (stateful, easy    │
                    │   CSRF protection)  │
                    │                     │
Mobile  ──Header──▶│  JWT-based Auth     │
API       Bearer    │  (stateless, cross  │
                    │   platform)         │
                    └─────────────────────┘
```

```csharp
// Support both mechanisms in the same app
builder.Services
    .AddAuthentication()
    .AddCookie("SessionScheme", options =>
    {
        options.LoginPath      = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer("JwtScheme", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ... JWT config
        };
    });

// Endpoint that supports both schemes
[Authorize(AuthenticationSchemes =
    "SessionScheme,JwtScheme")]
[HttpGet("profile")]
public IActionResult GetProfile()
{
    // Works with both browser (cookie session) and mobile (JWT header)
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return Ok(new { userId });
}
```

---

## Part 7 — Full Pattern Comparison

```
┌─────────────────┬──────────────┬────────────────┬──────────────────────┐
│                 │   Session +  │  JWT in        │  JWT Header          │
│                 │   Cookie     │  Cookie        │  (Bearer Token)      │
├─────────────────┼──────────────┼────────────────┼──────────────────────┤
│ State storage   │ Server       │ Client (token) │ Client (token)       │
│ XSS             │ ✅ Safe      │ ✅ Safe        │ ⚠️ If localStorage   │
│ CSRF            │ ⚠️ Protected │ ⚠️ Protected   │ ✅ Not vulnerable    │
│ Immediate revoke│ ✅           │ ⚠️ Needs DB    │ ⚠️ Needs DB          │
│ Scale           │ Needs store  │ ✅             │ ✅                   │
│ Mobile/API      │ ❌           │ ⚠️ Difficult   │ ✅                   │
│ Cross-domain    │ ❌           │ ❌             │ ✅                   │
│ Best for        │ Web apps     │ SPA web apps   │ API, Mobile          │
└─────────────────┴──────────────┴────────────────┴──────────────────────┘
```

---

## Summary

```
Cookie
  └── A transport mechanism, not authentication itself
      HttpOnly + Secure + SameSite = the security trio

Session-based
  └── Stateful — server stores state in a session store
      Easy to revoke, poor horizontal scalability, not mobile-friendly

Token-based (JWT)
  └── Stateless — client holds the token
      Easy to scale, suitable for mobile/API
      Cannot be revoked immediately

Hybrid patterns:
  ① JWT in HttpOnly Cookie
       Stateless + XSS protection → ideal for SPAs
  ② Access Token (15min) + Refresh Token (7 days) in Cookies
       Highest security, Refresh Token Rotation
  ③ Session for web + JWT for mobile
       Serves multiple client types from a single backend
```
