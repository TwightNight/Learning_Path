# Authentication & Authorization in ASP.NET Core

> **Version:** Complete & Extended — covers theory, JWT internals, hands-on implementation, and real-world authorization patterns.

---

## Section 1 — Authentication vs Authorization

These two concepts are frequently confused, but they are completely different responsibilities:

**Authentication** — *"Who are you?"*
The server verifies the caller's identity. Think of it as showing your ID card at a security gate.

**Authorization** — *"What are you allowed to do?"*
Once the server knows who you are, it checks whether you have permission to perform the requested action. Your ID card is valid, but it only grants access to the ground floor — not the VIP lounge.

```
Incoming request
    ↓
Authentication: "This token belongs to user ID=5, role=Customer" ✅
    ↓
Authorization:  "This endpoint requires role=Admin" ❌ → 403 Forbidden
```

The pipeline enforces this order: `UseAuthentication()` always runs before `UseAuthorization()`. Authentication establishes identity; Authorization consumes it.

---

## Section 2 — Common Authentication Methods

### 2.1 Session-based (traditional)

```
1. User logs in → server creates a Session, stores it in memory or a database
2. Server returns a Session ID to the client via a Set-Cookie header
3. On every subsequent request, the browser automatically sends the cookie
4. Server looks up the Session ID → identifies the user
```

**Drawbacks for Web APIs:** Not suitable for mobile apps. Does not scale well across multiple servers — sessions must be shared or replicated between nodes (sticky sessions or a shared session store).

### 2.2 JWT — JSON Web Token (modern, most widely used)

```
1. User logs in → server creates a JWT, signs it with a secret key
2. Server returns the token to the client
3. Client stores the token (localStorage, in-memory, etc.)
4. On every subsequent request, the client sends the token in the header:
   Authorization: Bearer <token>
5. Server verifies the signature → reads user information directly from the token
   (no database lookup required per request)
```

JWT is the preferred choice for Web APIs because it is **stateless** — the server stores nothing. All user information lives inside the token itself.

---

## Section 3 — How JWT Works Internally

### 3.1 JWT Structure

A JWT consists of three Base64URL-encoded parts separated by dots (`.`):

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9         ← Header
.eyJzdWIiOiI1IiwibmFtZSI6IkpvaG4gRG9lIiwicm9sZSI6IkN1c3RvbWVyIiwiZXhwIjoxNzE2MDAwMDAwfQ  ← Payload
.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c  ← Signature
```

**Header** — metadata describing the token itself:
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload** — the data embedded in the token, called **Claims**:
```json
{
  "sub": "5",
  "name": "John Doe",
  "email": "john@example.com",
  "role": "Customer",
  "exp": 1716000000
}
```

**Signature** — a cryptographic hash that proves the token has not been tampered with:
```
HMACSHA256(
  base64url(header) + "." + base64url(payload),
  secret_key
)
```

If anyone modifies the payload (e.g. changes `"role": "Customer"` to `"role": "Admin"`), the signature will no longer match the content — the server detects the mismatch immediately and rejects the token.

> **Key insight:** JWT payloads are Base64-encoded, not encrypted. Anyone can decode and read them. **Never put sensitive data** (passwords, card numbers, SSNs) in a JWT payload. The signature guarantees integrity (the data wasn't changed), not confidentiality (the data isn't readable).

### 3.2 What are Claims?

A **Claim** is a key-value pair embedded in the token that describes the authenticated user. There are three categories:

```csharp
// Registered claims — standardised, defined by the JWT specification
"sub"  → Subject (typically the user ID)
"exp"  → Expiration time (Unix timestamp when the token expires)
"iat"  → Issued at (Unix timestamp when the token was created)
"iss"  → Issuer (which application issued the token)
"aud"  → Audience (which application the token is intended for)
"jti"  → JWT ID (a unique identifier for this specific token — useful for revocation)

// Public claims — widely used conventions
"name", "email", "role"

// Private claims — custom, application-specific
"department", "branchId", "permissions", "subscriptionTier"
```

Claims are read back by the `UseAuthentication()` middleware after the token is validated, and are made available through `HttpContext.User` (a `ClaimsPrincipal`).

---

## Section 4 — Implementing JWT in ASP.NET Core

### 4.1 Install required packages

```
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

### 4.2 Configuration in `appsettings.json` and `Program.cs`

```json
// appsettings.json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-minimum-32-chars!!",
    "Issuer": "MyApp",
    "Audience": "MyAppUsers",
    "ExpiryMinutes": 60
  }
}
```

```csharp
// Program.cs
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate that the token was issued by our own application.
            // Rejects tokens from other apps even if the signature happens to match.
            ValidateIssuer = true,

            // Validate that the token is intended for this application.
            // Prevents tokens issued for one service from being used against another.
            ValidateAudience = true,

            // Validate that the token has not passed its expiry time (the "exp" claim).
            ValidateLifetime = true,

            // Validate the cryptographic signature — this is what prevents tampering.
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),

            // ClockSkew is a tolerance window added to the expiry time.
            // Default is 5 minutes — a token nominally expired for up to 5 minutes
            // is still accepted. Setting it to Zero means expired = rejected immediately.
            ClockSkew = TimeSpan.Zero
        };

        // Override default error responses so they return JSON instead of
        // the default WWW-Authenticate challenge header.
        options.Events = new JwtBearerEvents
        {
            // OnChallenge fires when the request is unauthenticated (missing or invalid token).
            // Without this override, ASP.NET Core returns a 401 with no response body.
            OnChallenge = context =>
            {
                context.HandleResponse(); // Suppress the default challenge response
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new
                {
                    status = 401,
                    message = "You are not logged in or your token is invalid."
                });
            },

            // OnForbidden fires when the user is authenticated but lacks the required role/policy.
            // Without this override, ASP.NET Core returns a 403 with no response body.
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new
                {
                    status = 403,
                    message = "You do not have permission to perform this action."
                });
            }
        };
    });

builder.Services.AddAuthorization();

// Pipeline — order is mandatory
app.UseAuthentication();   // Must come first: parse and validate the token
app.UseAuthorization();    // Must come second: check permissions using the parsed identity
```

### 4.3 AuthService — Creating and validating tokens

```csharp
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    string GenerateToken(User user);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly IUserRepository _userRepo;

    public AuthService(IConfiguration config, IUserRepository userRepo)
    {
        _config = config;
        _userRepo = userRepo;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Step 1: Find the user by email.
        // Use a vague error message ("email or password incorrect") — never tell the caller
        // which of the two fields is wrong, as that leaks account existence information.
        var user = await _userRepo.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedException("Email or password is incorrect.");

        // Step 2: Verify the password against the stored hash.
        // BCrypt.Verify() compares the plain-text password against a bcrypt hash.
        // NEVER compare plain-text passwords directly — always hash and compare.
        var passwordValid = BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
            throw new UnauthorizedException("Email or password is incorrect.");

        // Step 3: Generate the JWT access token.
        var token = GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto { Id = user.Id, Name = user.Name, Email = user.Email }
        };
    }

    public string GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        // Claims are key-value pairs embedded in the token payload.
        // The Authentication middleware extracts these after signature validation
        // and makes them available via HttpContext.User (ClaimsPrincipal).
        var claims = new[]
        {
            // "sub" (Subject) — the primary identifier of the user.
            // Conventionally the user's ID.
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),

            // ClaimTypes.Role is the claim that [Authorize(Roles = "...")] reads from.
            // If you use a custom claim name here, you must configure it in
            // TokenValidationParameters.RoleClaimType.
            new Claim(ClaimTypes.Role, user.Role),  // e.g. "Admin" or "Customer"

            // "jti" (JWT ID) — a unique identifier for this specific token.
            // Useful for token revocation: store revoked JTIs in a denylist and check
            // them in a custom middleware.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(jwtSettings["ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 4.4 AuthController

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // AuthService throws UnauthorizedException on invalid credentials,
        // which the global exception middleware converts to a 401 response.
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        // Hash the password before storing — NEVER store plain-text passwords.
        // BCrypt.HashPassword() generates a salted hash; each call produces a different
        // hash for the same input, making rainbow-table attacks impractical.
        var passwordHash = BCrypt.HashPassword(request.Password);

        // ... create and save the user with the hashed password
        return Created($"/api/users/{newUser.Id}", new UserDto(newUser));
    }
}
```

---

## Section 5 — Authorization: Controlling Access

### 5.1 `[Authorize]` — Require a logged-in user

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    // No attribute → accessible to anyone, including anonymous users.
    [HttpGet]
    public IActionResult GetAll() { ... }

    // [Authorize] → the request must carry a valid, non-expired JWT.
    // If no token or invalid token → 401 Unauthorized (not 403).
    [Authorize]
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto) { ... }

    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id) { ... }
}
```

Alternatively, apply `[Authorize]` at the controller level and carve out exceptions with `[AllowAnonymous]`:

```csharp
// Every endpoint in this controller requires authentication by default.
[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetMyOrders() { ... }   // requires authentication

    // [AllowAnonymous] overrides the controller-level [Authorize] for this endpoint only.
    // Useful for things like public statistics, terms of service, or health checks.
    [AllowAnonymous]
    [HttpGet("public-stats")]
    public IActionResult GetPublicStats() { ... }
}
```

### 5.2 Role-based Authorization

The simplest form of access control — restrict an endpoint to users whose token contains a specific role claim.

```csharp
// Only users with the "Admin" role can delete.
// A Customer who is authenticated will receive 403 Forbidden, not 401.
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public IActionResult Delete(int id) { ... }

// Accepts multiple roles separated by commas — this is a logical OR.
// Any user who is either an Admin OR a Manager can update.
[Authorize(Roles = "Admin,Manager")]
[HttpPut("{id}")]
public IActionResult Update(int id, UpdateProductDto dto) { ... }
```

The role value is read from the `ClaimTypes.Role` claim inside the JWT. The value must match exactly (case-sensitive by default).

### 5.3 Policy-based Authorization

Role-based authorization covers simple cases well, but it becomes unwieldy when access rules depend on multiple conditions (role + subscription tier + age + etc.). **Policies** let you define complex, reusable rules in one place.

```csharp
// Define policies in Program.cs — centralised, testable, and named.
builder.Services.AddAuthorization(options =>
{
    // Simple role-based policy. Equivalent to [Authorize(Roles = "Admin")] but
    // more flexible because the policy can be upgraded later without touching every
    // controller attribute.
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Claim-based policy with a custom assertion.
    // RequireClaim("age") ensures the claim exists, then RequireAssertion checks its value.
    // Separating existence and value checks makes the failure reason easier to diagnose.
    options.AddPolicy("MinimumAge18", policy =>
        policy.RequireClaim("age")
              .RequireAssertion(ctx =>
              {
                  var ageClaim = ctx.User.FindFirst("age");
                  return ageClaim != null && int.TryParse(ageClaim.Value, out var age)
                         && age >= 18;
              }));

    // Compound policy: must satisfy BOTH conditions simultaneously (logical AND).
    // The user must be a Customer AND have a "premium" subscription claim.
    options.AddPolicy("PremiumUser", policy =>
        policy.RequireRole("Customer")
              .RequireClaim("subscription", "premium"));

    // Flexible assertion: Admin can do anything; non-Admin users can only act on their
    // own resources. The resource value is passed in when calling IAuthorizationService
    // programmatically (see Resource-based Authorization below).
    options.AddPolicy("SameUserOrAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Admin") ||
            ctx.User.HasClaim(ClaimTypes.NameIdentifier,
                ctx.Resource?.ToString() ?? "")));
});
```

```csharp
// Apply the policy via the attribute — clean and declarative.
[Authorize(Policy = "AdminOnly")]
[HttpDelete("{id}")]
public IActionResult Delete(int id) { ... }

[Authorize(Policy = "PremiumUser")]
[HttpGet("premium-content")]
public IActionResult GetPremiumContent() { ... }
```

### 5.4 Resource-based Authorization

Used when access rules depend on the **specific content** of a resource — not just the user's role or claims. For example: "a user may only edit their own post, but an Admin can edit any post."

The check happens **inside the action method** after the resource has been loaded from the database, because the ownership information only exists at that point.

```csharp
[Authorize]
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, UpdatePostDto dto)
{
    var post = await _postRepo.GetByIdAsync(id);
    if (post == null) return NotFound();

    // Extract the authenticated user's ID from the token claims.
    // FindFirstValue returns null if the claim doesn't exist — the ! suppresses the
    // compiler warning because [Authorize] guarantees the user is authenticated.
    var currentUserId = int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Inline ownership check: the user owns this post, OR is an Admin.
    // Forbid() returns 403 Forbidden (unlike Unauthorized() which returns 401).
    if (post.AuthorId != currentUserId && !User.IsInRole("Admin"))
        return Forbid();

    // ... proceed with the update
    await _postRepo.UpdateAsync(post, dto);
    return NoContent();
}
```

For more complex resource-based rules, use `IAuthorizationService` with a custom `IAuthorizationHandler` to keep authorization logic out of controllers entirely.

---

## Section 6 — Reading User Information from the Token in Controllers

After `UseAuthentication()` runs, the JWT is validated and its claims are decoded and attached to `HttpContext.User` (a `ClaimsPrincipal`). Inside any controller action, these claims are accessible through the `User` property:

```csharp
[Authorize]
[HttpGet("me")]
public IActionResult GetMyProfile()
{
    // FindFirstValue searches the ClaimsPrincipal for a claim with the given type
    // and returns its value as a string, or null if the claim is not present.
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // maps to "sub"
    var email  = User.FindFirstValue(ClaimTypes.Email);
    var name   = User.FindFirstValue(ClaimTypes.Name);
    var role   = User.FindFirstValue(ClaimTypes.Role);

    // IsInRole is a convenience method — equivalent to checking the Role claim value.
    var isAdmin = User.IsInRole("Admin");

    return Ok(new { userId, email, name, role, isAdmin });
}
```

> **Tip:** Create an extension method or a `CurrentUserService` (injected via DI) to centralise claim extraction — avoids repeating `User.FindFirstValue(ClaimTypes.NameIdentifier)` in every action that needs the current user's ID.

```csharp
// CurrentUserService — a cleaner alternative to reading claims in every controller
public interface ICurrentUserService
{
    int UserId { get; }
    string Email { get; }
    bool IsAdmin { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Parse once, reuse everywhere. Controllers inject ICurrentUserService and call
    // _currentUser.UserId instead of repeating the FindFirstValue pattern.
    public int UserId => int.Parse(
        _httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

    public string Email =>
        _httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.Email)!;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext!.User.IsInRole("Admin");
}

// Register in Program.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

---

## Section 7 — Refresh Tokens

**Why they exist:** Access tokens are short-lived (typically 15–60 minutes) to limit the damage if a token is stolen — the attacker can only use it for a short window. However, forcing users to log in again every hour is terrible UX. Refresh tokens solve this by allowing silent token renewal without re-entering credentials.

**The flow:**

```
1. User logs in → server returns:
   - Access Token  (expires in 15 minutes)
   - Refresh Token (expires in 7 days, stored in the database)

2. Client uses the Access Token to call protected API endpoints.

3. Access Token expires → client calls POST /api/auth/refresh
   with the Refresh Token in the request body.

4. Server looks up the Refresh Token in the database:
   - Valid and not expired → issue a new Access Token + a new Refresh Token
   - Not found or expired   → return 401, force the user to log in again

5. Server deletes the old Refresh Token from the database,
   saves the new one (token rotation — prevents replay attacks).
```

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
{
    // Step 1: Look up the provided Refresh Token in the database.
    // If it doesn't exist, it was either already used (token rotation) or never issued.
    var stored = await _tokenRepo.FindAsync(request.RefreshToken);

    if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
    {
        // Return 401 for both "not found" and "expired" — don't reveal which.
        return Unauthorized(new { message = "Refresh token is invalid or has expired." });
    }

    // Step 2: Load the associated user to generate a fresh set of claims.
    // This also picks up any role or permission changes made since the last login.
    var user = await _userRepo.GetByIdAsync(stored.UserId);

    // Step 3: Generate a new Access Token and a new Refresh Token.
    var newAccessToken = _authService.GenerateToken(user);
    var newRefreshToken = Guid.NewGuid().ToString("N"); // cryptographically random string

    // Step 4: Delete the old Refresh Token and save the new one (token rotation).
    // If an attacker tries to reuse the old Refresh Token, it will no longer exist
    // in the database — this is the key mechanism that detects token theft.
    await _tokenRepo.DeleteAsync(stored.Id);
    await _tokenRepo.SaveAsync(new RefreshToken
    {
        Token = newRefreshToken,
        UserId = user.Id,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    });

    return Ok(new
    {
        accessToken = newAccessToken,
        refreshToken = newRefreshToken
    });
}
```

> **Security note:** Store Refresh Tokens as hashed values in the database (same principle as passwords), so that a database breach doesn't expose live tokens. When the client sends a Refresh Token, hash it and compare against the stored hash.

---

## Section 8 — Full Authentication & Authorization Flow

```
Client sends: GET /api/orders  +  Header: "Authorization: Bearer <token>"
        │
        ▼
[Authentication Middleware]  (UseAuthentication)
  ├── Extract the token from the Authorization header
  ├── Verify the cryptographic signature using the secret key
  ├── Check the "exp" claim — reject if expired
  ├── Decode all claims → populate HttpContext.User (ClaimsPrincipal)
  └── If token is missing or invalid → context.User = anonymous (not authenticated)
        │
        ▼
[Authorization Middleware]  (UseAuthorization)
  ├── Does the matched endpoint have [Authorize]?
  │     └── No → let the request through (public endpoint)
  ├── Is context.User authenticated?
  │     └── No → 401 Unauthorized
  ├── Does [Authorize] specify a Role?
  │     └── User doesn't have that role → 403 Forbidden
  ├── Does [Authorize] specify a Policy?
  │     └── Policy assertion fails → 403 Forbidden
  └── All checks pass → forward to the Action Method
        │
        ▼
Action Method
  └── Read user information via User.FindFirstValue(...)
      or inject ICurrentUserService
```

---

## Summary

```
Authentication — "Who are you?"
  └── JWT: user logs in → receives a signed token → sends it with every request

JWT structure (3 parts separated by dots):
  Header . Payload (Claims) . Signature

  Header    → algorithm and token type
  Payload   → user data (ID, email, role, expiry) — NOT encrypted, just encoded
  Signature → cryptographic proof the payload was not tampered with

Authorization — "What are you allowed to do?"
  ├── [Authorize]                    → must be authenticated
  ├── [Authorize(Roles = "X")]       → must have role X
  ├── [Authorize(Policy = "X")]      → must satisfy policy X
  ├── [AllowAnonymous]               → explicitly permit unauthenticated access
  └── Resource-based (in-code)       → check ownership after loading the resource

Refresh Tokens — sustain long-lived sessions without re-authentication
  ├── Short-lived Access Token (15–60 min) for API calls
  └── Long-lived Refresh Token (7 days) for silent renewal — stored in DB

Common mistakes:
  ├── Storing sensitive data in JWT payload  → it is readable by anyone who has the token
  ├── Comparing plain-text passwords         → always hash with BCrypt/Argon2
  ├── UseAuthorization before UseAuthentication → permanent 401 for all requests
  └── Reusing Refresh Tokens (no rotation)  → compromised token can be used indefinitely
```

---
