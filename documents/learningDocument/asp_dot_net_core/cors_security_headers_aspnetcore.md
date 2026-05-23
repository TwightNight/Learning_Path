# CORS & Security Headers in ASP.NET Core

> **Version:** Complete & Extended — covers the Same-Origin Policy, how CORS works under the hood, full ASP.NET Core configuration, and every important security response header with practical context.

---

## Section 1 — Same-Origin Policy: The Root of the Problem

### 1.1 What is an Origin?

**Origin** = Scheme + Host + Port

```
https://myapp.com:443/api/products
│       │           │
Scheme  Host        Port
└───────┴───────────┘
        Origin = https://myapp.com
```

Two URLs share the same origin only when **all three components are identical**:

```
https://myapp.com/api/users       ✅ Same origin
https://myapp.com/dashboard       ✅ Same origin

http://myapp.com/api              ❌ Different scheme (http vs https)
https://api.myapp.com/users       ❌ Different host (subdomain counts)
https://myapp.com:8080/api        ❌ Different port
https://otherapp.com/api          ❌ Entirely different host
```

### 1.2 Same-Origin Policy (SOP)

**SOP** is a security policy **built into every browser**: a web page may only read the response from a request if the request goes to the **same origin** as the page itself.

```
Browser on https://frontend.com sends fetch() to https://api.com/data
                                                   ↑ different origin

Browser: "Different origin — I will block the JavaScript from reading this response."
```

SOP protects users from malicious pages silently reading data from other sites (e.g. your email inbox, bank balance) using the user's active session cookies.

> **Critical distinction:** SOP only applies **inside a browser**. Requests from Postman, mobile apps, curl, or server-to-server calls are completely unaffected. CORS errors are purely a browser-side enforcement mechanism.

---

## Section 2 — What is CORS?

**CORS (Cross-Origin Resource Sharing)** is a mechanism that allows a server to **relax SOP in a controlled way** — the server tells the browser: *"I trust this origin; allow its JavaScript to read my responses."*

CORS works through a negotiation of **HTTP headers** exchanged between the browser and the server. The server is always in control — it decides which origins, methods, and headers it accepts.

---

## Section 3 — How CORS Works Internally

### 3.1 Simple Request

For requests considered "simple" by the browser (GET, POST with `Content-Type: text/plain` or `application/x-www-form-urlencoded`), the browser sends the request immediately and appends an `Origin` header automatically:

```
Browser → Server:
GET /api/products HTTP/1.1
Origin: https://frontend.com       ← added by the browser automatically
Host: api.myapp.com

Server → Browser:
HTTP/1.1 200 OK
Access-Control-Allow-Origin: https://frontend.com   ← server permits this origin
Content-Type: application/json
[response body...]

Browser checks the response header:
  Access-Control-Allow-Origin matches the Origin sent? → ✅ allow JS to read the response
  Header missing or value doesn't match?               → ❌ block — JS cannot read the response
```

Note: the request *was sent and processed* by the server either way. SOP only controls whether the browser **exposes the response** to JavaScript — it does not prevent the request from reaching the server.

### 3.2 Preflight Request

For requests the browser considers "complex" (PUT, DELETE, PATCH, or `Content-Type: application/json`, or custom headers like `Authorization`), the browser **automatically sends an `OPTIONS` request first** to ask the server for permission before making the real request:

```
Step 1 — Browser automatically sends a Preflight:
OPTIONS /api/products/5 HTTP/1.1
Origin: https://frontend.com
Access-Control-Request-Method: DELETE
Access-Control-Request-Headers: Content-Type, Authorization

Step 2 — Server responds to the Preflight:
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: https://frontend.com
Access-Control-Allow-Methods: GET, POST, PUT, DELETE
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Max-Age: 86400     ← cache this preflight result for 24 hours

Step 3 — Browser received approval, now sends the real request:
DELETE /api/products/5 HTTP/1.1
Origin: https://frontend.com
Authorization: Bearer eyJ...

Step 4 — Server processes the real request and returns the response normally.
```

If the server does not respond correctly to the preflight (missing headers, wrong values, or non-2xx status) → the browser does not send the real request → JavaScript sees a CORS error.

> **Performance note:** `Access-Control-Max-Age` tells the browser to cache the preflight result. Without this, the browser sends a preflight before *every single* complex request, doubling your network round-trips. Set it to a high value (e.g. 86400 = 24 hours) for stable APIs.

### 3.3 Key CORS Response Headers

| Header | Purpose |
|--------|---------|
| `Access-Control-Allow-Origin` | Which origin(s) are permitted |
| `Access-Control-Allow-Methods` | Which HTTP methods are permitted |
| `Access-Control-Allow-Headers` | Which request headers the client may send |
| `Access-Control-Expose-Headers` | Which response headers JavaScript may read (beyond the default safe list) |
| `Access-Control-Allow-Credentials` | Whether cookies and `Authorization` headers may be sent cross-origin |
| `Access-Control-Max-Age` | How many seconds the preflight result may be cached |

---

## Section 4 — Configuring CORS in ASP.NET Core

### 4.1 Basic configuration

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    // Production policy: strict allowlist of known frontend origins.
    // Any origin not listed here will receive no CORS headers → blocked by the browser.
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://myapp.com",
                "https://www.myapp.com",
                "https://admin.myapp.com")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders("Content-Type", "Authorization", "X-Request-Id")
            // Headers exposed to JavaScript beyond the default safe list.
            // Useful for pagination metadata (total count, page size) in response headers.
            .WithExposedHeaders("X-Total-Count", "X-Page-Size")
            // Cache preflight for 24 hours — avoids an OPTIONS round-trip on every request.
            .SetPreflightMaxAge(TimeSpan.FromHours(24));
    });

    // Development policy: permissive, allows local frontend dev servers.
    // The specific localhost ports match common Vite (5173) and CRA (3000) defaults.
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();   // allows sending cookies cross-origin (for dev auth flows)
    });
});

// Apply in the pipeline — after UseRouting(), before UseAuthentication().
// Automatically selects the right policy based on the environment.
var policy = app.Environment.IsDevelopment()
    ? "DevelopmentPolicy"
    : "ProductionPolicy";

app.UseCors(policy);
```

### 4.2 `AllowCredentials` — A Special Case

When you need the browser to include cookies or `Authorization` headers in a cross-origin request, you must add `AllowCredentials()`. However, this **cannot be combined with `AllowAnyOrigin()`** — the browser will reject the response if both are present simultaneously, because allowing credentials to any arbitrary origin would be a critical security vulnerability.

```csharp
// ❌ Runtime error — these two cannot be combined.
// ASP.NET Core throws an InvalidOperationException at startup.
policy.AllowAnyOrigin().AllowCredentials();

// ✅ Correct — must specify explicit origins when using credentials.
policy.WithOrigins("https://myapp.com").AllowCredentials();
```

### 4.3 Per-endpoint CORS policies

Different parts of your API may need different CORS rules — a public widget API vs. an internal admin API, for example. Register multiple named policies and apply them selectively:

```csharp
// Register multiple named policies in Program.cs
builder.Services.AddCors(options =>
{
    // Open to any origin — for public SDK/widget endpoints
    options.AddPolicy("PublicApi", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

    // Restricted to one internal dashboard origin
    options.AddPolicy("InternalOnly", policy =>
        policy.WithOrigins("https://internal.myapp.com")
              .AllowAnyMethod()
              .AllowCredentials());
});

// Apply per controller via attribute — overrides any global UseCors() policy
[ApiController]
[Route("api/public")]
[EnableCors("PublicApi")]
public class PublicController : ControllerBase { ... }

[ApiController]
[Route("api/internal")]
[EnableCors("InternalOnly")]
public class InternalController : ControllerBase { ... }

// Opt out of CORS entirely for a specific endpoint
// (e.g. a server-to-server endpoint that should never be called from a browser)
[DisableCors]
[HttpGet("server-only")]
public IActionResult ServerOnly() { ... }
```

### 4.4 Dynamic CORS — origins from configuration or a database

When you cannot know the allowed origins at startup (e.g. a multi-tenant SaaS where tenants register their own frontend domains), use `SetIsOriginAllowed()` for runtime evaluation:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DynamicPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Allow any subdomain of myapp.com (e.g. tenant1.myapp.com, tenant2.myapp.com)
            // without needing to enumerate them at startup.
            var uri = new Uri(origin);
            return uri.Host.EndsWith(".myapp.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.Equals("myapp.com", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

For origins stored in a database, inject a scoped service inside `SetIsOriginAllowedToAllowWildcardSubdomains` or resolve origins at startup via `IServiceProvider.CreateScope()`.

---

## Section 5 — Security Headers

CORS addresses the cross-origin access problem. But there are many other attack vectors — clickjacking, XSS, MIME sniffing, HTTPS downgrade attacks — that CORS does nothing to prevent. **Security headers** provide a second layer of defence by instructing the browser on how to handle your responses.

### 5.1 Implementing security headers as middleware

Either install the `NWebsec.AspNetCore.Middleware` package, or write a simple inline middleware (recommended for full control):

```csharp
// dotnet add package NWebsec.AspNetCore.Middleware  (optional — shown below manually)

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // 1. Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // 2. Prevent MIME sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // 3. Control referrer information leakage
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // 4. Legacy XSS filter (for older browsers — modern browsers use CSP instead)
        headers["X-XSS-Protection"] = "1; mode=block";

        // 5. Disable browser APIs your app does not use
        headers["Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=(), payment=()";

        // 6. HSTS — only add on HTTPS responses (production only)
        if (context.Request.IsHttps)
            headers["Strict-Transport-Security"] =
                "max-age=31536000; includeSubDomains; preload";

        // 7. Content Security Policy — the most powerful protection against XSS
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'";

        // Remove headers that reveal server implementation details.
        // Attackers use these to fingerprint the server and look up known vulnerabilities.
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");

        await _next(context);
    }
}
```

Register it in `Program.cs`, early in the pipeline, before `UseStaticFiles()`:

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
```

---

### 5.2 Each Security Header Explained

---

#### ① X-Frame-Options — Prevent Clickjacking

**The attack:** Clickjacking embeds your page inside an invisible `<iframe>` on a malicious site, positioned over a fake UI. The victim thinks they are clicking a harmless button, but they are actually clicking a button on your page (e.g. "Confirm Transfer", "Grant Permission").

```
X-Frame-Options: DENY         → nobody may embed this page in an iframe, anywhere
X-Frame-Options: SAMEORIGIN   → only the same origin may embed this page in an iframe
```

> **Modern alternative:** `frame-ancestors` in the Content Security Policy header supersedes `X-Frame-Options` and is more flexible. Set both for maximum browser compatibility.

---

#### ② X-Content-Type-Options — Prevent MIME Sniffing

**The attack:** Browsers sometimes guess the content type of a response by inspecting its content (MIME sniffing) rather than trusting the `Content-Type` header. An attacker can upload a file that *looks* like an image but contains executable JavaScript. The browser sniffs it as JavaScript and runs it.

```
X-Content-Type-Options: nosniff   → force the browser to trust the Content-Type header exactly
                                     and never sniff or reinterpret the content
```

---

#### ③ Strict-Transport-Security (HSTS) — Enforce HTTPS

**The problem:** Even if your server always redirects HTTP to HTTPS, the very first request a user makes might be over HTTP — giving a network attacker a window to intercept or downgrade the connection.

**How HSTS works:** After the first successful HTTPS response, the browser notes the `max-age` and for that duration automatically upgrades all subsequent requests to HTTPS — without ever sending an HTTP request, even if the user types `http://`.

```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload

max-age=31536000   → remember this policy for 1 year
includeSubDomains  → apply to all subdomains (api.myapp.com, admin.myapp.com, etc.)
preload            → register in browser vendor preload lists — HSTS is enforced
                     even on the very first ever visit, before the server responds
```

> **Warning:** Only enable HSTS in production and only when you are certain every part of your domain (including all subdomains if you use `includeSubDomains`) is served over valid HTTPS. Enabling it incorrectly locks users out of your site for the entire `max-age` duration with no easy recovery. Test thoroughly in staging first.

---

#### ④ Content Security Policy (CSP) — The Most Powerful XSS Defence

**The problem it solves:** Cross-Site Scripting (XSS) occurs when an attacker injects malicious script into your page — either through user-generated content (stored XSS), URL parameters (reflected XSS), or DOM manipulation. Once injected, the script runs with the full permissions of your page.

**How CSP works:** CSP is an allowlist that tells the browser exactly which sources it may load scripts, styles, images, fonts, and connections from. Any resource not on the allowlist is blocked — even if an attacker somehow injects a `<script src="https://evil.com/steal.js">` tag, the browser refuses to load it.

```
Content-Security-Policy directives:
  default-src 'self'                    → default: only load from the same origin
  script-src 'self'                     → scripts: same origin only (no inline scripts)
  style-src 'self' 'unsafe-inline'      → styles: same origin + inline styles (needed
                                          for many CSS frameworks; remove if possible)
  img-src 'self' data: https:           → images: same origin, data URIs, any HTTPS source
  font-src 'self'                       → fonts: same origin only
  connect-src 'self'                    → fetch/XHR/WebSocket: same origin only
  frame-ancestors 'none'                → nobody may embed this page in an iframe
                                          (replaces X-Frame-Options)
  base-uri 'self'                       → prevent <base> tag injection (base tag hijacking)
  form-action 'self'                    → form submissions only to the same origin
  upgrade-insecure-requests             → automatically upgrade HTTP sub-resources to HTTPS
```

**Real-world CSP for a SPA using Google Fonts and Stripe:**

```
Content-Security-Policy:
  default-src 'self';
  script-src 'self' https://js.stripe.com;
  style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
  font-src 'self' https://fonts.gstatic.com;
  img-src 'self' data: https:;
  connect-src 'self' https://api.stripe.com https://api.myapp.com;
  frame-src https://js.stripe.com;
  frame-ancestors 'none';
  base-uri 'self';
  form-action 'self'
```

> **Tip — CSP Report-Only mode:** Before enforcing CSP in production, use `Content-Security-Policy-Report-Only` with a `report-uri` to collect violations without blocking anything. This lets you identify all the sources your app actually needs before you lock it down.
>
> ```
> Content-Security-Policy-Report-Only: default-src 'self'; report-uri /csp-violations
> ```

---

#### ⑤ Referrer-Policy — Control Information Leakage via Referrer

When a user follows a link or the browser loads a sub-resource, it typically sends a `Referer` header with the URL of the page that initiated the request. This can leak private URL parameters (e.g. `/reset-password?token=abc123`) to third-party domains.

```
Referrer-Policy: no-referrer                       → never send any Referer header
Referrer-Policy: strict-origin-when-cross-origin   → send full URL for same-origin requests;
                                                      send only the origin for cross-origin;
                                                      send nothing over HTTP→HTTPS transitions
Referrer-Policy: same-origin                       → only send Referer for same-origin requests
Referrer-Policy: no-referrer-when-downgrade        → (old default) send full URL for same-scheme,
                                                      nothing for HTTPS→HTTP
```

`strict-origin-when-cross-origin` is the recommended default for most applications.

---

#### ⑥ Permissions-Policy — Disable Unused Browser APIs

Limits which powerful browser APIs (camera, microphone, geolocation, etc.) your page and its embedded frames may use. This limits the damage if an XSS attack succeeds or a third-party script misbehaves — even if JavaScript runs, it cannot access the camera if the Permissions Policy blocks it.

```
Permissions-Policy:
  camera=()             → camera API disabled entirely
  microphone=()         → microphone API disabled entirely
  geolocation=()        → geolocation API disabled entirely
  payment=()            → Payment Request API disabled entirely
  usb=()                → WebUSB API disabled entirely
  interest-cohort=()    → FLoC tracking disabled (now deprecated but still worth setting)
```

---

### 5.3 Complete configuration in ASP.NET Core

Using `Response.OnStarting()` is preferable to setting headers directly before `next()` because it runs just before headers are flushed — avoiding the risk of the middleware adding headers after they have already been sent:

```csharp
// Program.cs — place this early in the pipeline, before UseStaticFiles
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var h = context.Response.Headers;

        // Always apply these regardless of environment
        h["X-Frame-Options"]        = "DENY";
        h["X-Content-Type-Options"] = "nosniff";
        h["X-XSS-Protection"]       = "1; mode=block";
        h["Referrer-Policy"]        = "strict-origin-when-cross-origin";
        h["Permissions-Policy"]     = "camera=(), microphone=(), geolocation=()";

        // HSTS: only on HTTPS responses (never on HTTP in development)
        if (context.Request.IsHttps)
            h["Strict-Transport-Security"] =
                "max-age=31536000; includeSubDomains";

        // CSP: only add if not already set (allows per-controller overrides)
        if (!h.ContainsKey("Content-Security-Policy"))
            h["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'";

        // Remove server fingerprinting headers
        h.Remove("Server");
        h.Remove("X-Powered-By");
        h.Remove("X-AspNet-Version");
        h.Remove("X-AspNetMvc-Version");

        return Task.CompletedTask;
    });

    await next();
});
```

---

## Section 6 — Verifying Your Security Headers

After deploying, check your headers at [securityheaders.com](https://securityheaders.com) — a free tool that grades each header, explains what is missing, and suggests improvements.

Target result:

```
✅ A+  Content-Security-Policy
✅ A+  Strict-Transport-Security
✅ A+  X-Frame-Options
✅ A+  X-Content-Type-Options
✅ A+  Referrer-Policy
✅ A+  Permissions-Policy
```

Also check [observatory.mozilla.org](https://observatory.mozilla.org) for a second opinion — Mozilla's tool additionally checks cookie flags (`Secure`, `HttpOnly`, `SameSite`) and subresource integrity.

---

## Section 7 — Full CORS + Security Headers Flow

```
Request from browser (https://frontend.com)
              │
              ▼
    ┌────────────────────────────┐
    │  Is this a Preflight?       │ ← browser sends OPTIONS first for complex requests
    │  CORS check:                │
    │  Is Origin in the whitelist?│
    │    Yes → 204 + CORS headers │
    │    No  → 403, no headers    │ ← browser blocks the real request
    └──────────┬─────────────────┘
               │ (preflight approved, or simple request)
               ▼
    ┌────────────────────────────┐
    │  Security Headers           │
    │  Middleware                 │ ← adds X-Frame-Options, CSP, HSTS, etc.
    │  (added to every response)  │   to every response going back to the browser
    └──────────┬─────────────────┘
               ▼
    ┌────────────────────────────┐
    │  Authentication /           │
    │  Authorization              │
    └──────────┬─────────────────┘
               ▼
    ┌────────────────────────────┐
    │  Action Method              │
    │  → Response body            │
    │  + all security headers     │
    └────────────────────────────┘
```

---

## Summary

```
Same-Origin Policy (SOP)
  └── Built into every browser: JavaScript may not read cross-origin responses
      Only applies in browsers — Postman, mobile apps, server calls are unaffected

CORS — controlled SOP relaxation
  ├── Simple Request     → sent immediately; browser checks Origin in response
  ├── Preflight          → OPTIONS first → server approves → real request sent
  ├── AllowCredentials   → requires WithOrigins() — cannot use AllowAnyOrigin()
  ├── Max-Age            → cache preflight to avoid repeated OPTIONS round-trips
  └── Configuration      → named policies → UseCors() or [EnableCors("name")]

Security Headers — defence in depth
  ├── X-Frame-Options          → prevents clickjacking (iframing your page)
  ├── X-Content-Type-Options   → prevents MIME sniffing attacks
  ├── HSTS                     → enforces HTTPS, even on first visit (with preload)
  ├── CSP                      → allowlists resource sources — most powerful XSS defence
  ├── Referrer-Policy          → limits URL leakage via the Referer header
  └── Permissions-Policy       → disables unused browser APIs (camera, mic, location)

Common mistakes:
  ├── AllowAnyOrigin + AllowCredentials  → runtime exception, and a security hole
  ├── HSTS on HTTP responses             → header is ignored; always check IsHttps
  ├── Missing preflight Max-Age          → OPTIONS request sent on every complex call
  └── CSP with 'unsafe-inline' scripts  → greatly weakens XSS protection
        use nonces or hashes instead when possible
```

---

> **What to study next:** **Entity Framework Core** (ORM, `DbContext`, Migrations, real database CRUD — the most common Scoped service in any ASP.NET Core app), or **Error Handling & Response Standardisation** (ProblemDetails, global exception middleware, consistent API response envelopes).
