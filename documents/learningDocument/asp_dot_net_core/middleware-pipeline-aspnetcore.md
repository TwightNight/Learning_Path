# Middleware & Pipeline in ASP.NET Core

---

## Section 1 — What is a Pipeline?

When an HTTP request arrives at the server, it does not go directly into an action method. It must pass through a **chain of processing layers** — that chain is the **Pipeline**.

Think of it like an assembly line: each station (middleware) performs a specific task, then passes the product to the next station. Once done, the result travels back through each station in reverse order.

```
                    Request →
Client ──→ [Middleware 1] → [Middleware 2] → [Middleware 3] → Action Method
Client ←── [Middleware 1] ← [Middleware 2] ← [Middleware 3] ←
                    ← Response
```

This is a **"two-way pipe"** model — the request flows in, the response flows out, and whichever middleware is registered first **wraps the outermost layer**.

> **Important note:** The pipeline in ASP.NET Core is built once at startup (when `builder.Build()` is called). Once the application is running, the pipeline is **immutable** — you cannot add or remove middleware at runtime. Any changes require an application restart.

---

## Section 2 — What is Middleware?

**Middleware** is a piece of code that sits inside the pipeline and can:

- Read and modify the **request** before passing it to the next layer
- Read and modify the **response** before returning it to the client
- **Short-circuit the pipeline** — stop forwarding to the next layer
- Pass control to the next middleware by calling `next()`

Every middleware receives an `HttpContext` — the object that holds all information about the current request and response.

### Middleware is a delegate

At its core, every middleware is a `RequestDelegate`:

```csharp
public delegate Task RequestDelegate(HttpContext context);
```

The entire pipeline is a chain of `RequestDelegate` instances nested inside one another in registration order.

---

## Section 3 — Middleware Structure

### 3.1 Inline style (written directly in Program.cs)

Suitable for simple logic or quick experimentation.

```csharp
app.Use(async (context, next) =>
{
    // Code runs when the request COMES IN
    Console.WriteLine($"→ Request: {context.Request.Method} {context.Request.Path}");

    await next(context);   // Forward to the next middleware

    // Code runs when the response COMES OUT (after all downstream middleware finishes)
    Console.WriteLine($"← Response: {context.Response.StatusCode}");
});
```

### 3.2 Class style (standard, recommended for production)

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // _next is a delegate pointing to the next middleware in the pipeline.
    // The constructor is called ONCE at startup → effectively Singleton lifecycle.
    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // InvokeAsync is called on every request — this is the main processing point.
    public async Task InvokeAsync(HttpContext context)
    {
        // ── Before forwarding ───────────────────────────
        var sw = Stopwatch.StartNew();
        _logger.LogInformation(
            "→ {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await _next(context);   // ← key point: invoke the next middleware

        // ── After downstream middleware finishes ────────
        sw.Stop();
        _logger.LogInformation(
            "← {StatusCode} after {Ms}ms",
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}

// Extension method for cleaner registration
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(
        this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
```

Register in `Program.cs`:

```csharp
app.UseRequestLogging();
```

### 3.3 Middleware with Scoped dependencies

The middleware constructor is called **only once** (Singleton lifecycle), but `InvokeAsync` is called per request. If you need to inject a Scoped service (e.g. `DbContext`), **do not inject it through the constructor** — inject it directly as a parameter of `InvokeAsync`:

```csharp
public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // ASP.NET Core automatically resolves Scoped services injected into InvokeAsync.
    // This is safe because InvokeAsync runs per-request inside a fresh DI scope.
    public async Task InvokeAsync(HttpContext context, AuditDbContext db)
    {
        await _next(context);

        // Write an audit log to the database after the request completes
        db.AuditLogs.Add(new AuditLog
        {
            Path = context.Request.Path,
            StatusCode = context.Response.StatusCode,
            Timestamp = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
```

> **Rule of thumb:** Singleton services → constructor. Scoped/Transient services → `InvokeAsync` parameters.

---

## Section 4 — Middleware Order — The Most Important Thing

**The order in which middleware is registered determines the order of execution.** This is the most common source of bugs for newcomers.

```csharp
// Program.cs — this order matters enormously
var app = builder.Build();

app.UseExceptionHandler("/error");  // 1. Global exception handler — must be first
app.UseHttpsRedirection();          // 2. Redirect HTTP → HTTPS
app.UseStaticFiles();               // 3. Serve static files (css, js, images) — short-circuits if matched
app.UseRouting();                   // 4. Match the incoming URL to an endpoint
app.UseCors();                      // 5. CORS check — must come after Routing
app.UseAuthentication();            // 6. Identify "who are you?"
app.UseAuthorization();             // 7. Check "do you have permission?" — must come after Authentication
app.UseRateLimiter();               // 8. Throttle incoming requests (ASP.NET Core 7+)
app.MapControllers();               // 9. Terminal — invokes the matched action method
```

**Detailed execution flow:**

```
Incoming request
    ↓
[ExceptionHandler]  ← outermost wrapper, catches any unhandled error below
    ↓
[HttpsRedirection]
    ↓
[StaticFiles]       ← serves file immediately if matched, pipeline stops here
    ↓
[Routing]           ← must run before Cors/Auth so the endpoint is known
    ↓
[Cors]
    ↓
[Authentication]    ← must know who the user is before...
    ↓
[Authorization]     ← ...checking whether they have permission
    ↓
[MapControllers]    ← action method runs last
```

**Common mistakes caused by wrong order:**

| Symptom | Root cause |
|---------|------------|
| Always getting 401 even with a valid token | `UseAuthorization()` placed before `UseAuthentication()` |
| CORS headers missing from response | `UseCors()` placed before `UseRouting()` |
| Static files not being served | `UseStaticFiles()` placed after `UseRouting()` |
| Exceptions not caught properly | `UseExceptionHandler()` is not the first middleware |
| Cannot modify StatusCode in error handler | Response already started (`HasStarted = true`) |

---

## Section 5 — Five Middleware Registration Variants

### 5.1 `app.Use()` — Pass-through middleware

```csharp
app.Use(async (context, next) =>
{
    // do something before
    await next(context);   // required to continue the pipeline
    // do something after downstream middleware finishes
});
```

### 5.2 `app.Run()` — Terminal middleware, stops the pipeline

```csharp
app.Run(async context =>
{
    // No next parameter — pipeline ends here
    await context.Response.WriteAsync("Pipeline ends here");
});
```

Use `app.Run()` at the end of the pipeline or for intentional short-circuiting.

> **Note:** Any middleware registered **after** `app.Run()` will never execute.

### 5.3 `app.Map()` — Branch pipeline by URL path

```csharp
// Separate branch for /api/health
app.Map("/api/health", healthApp =>
{
    healthApp.Run(async context =>
    {
        await context.Response.WriteAsync("{ \"status\": \"healthy\" }");
    });
});

// Requests that don't match /api/health continue through the main pipeline
app.UseAuthentication();
app.MapControllers();
```

### 5.4 `app.MapWhen()` — Branch by arbitrary condition

```csharp
// Branch based on any condition derived from HttpContext
app.MapWhen(
    context => context.Request.Headers.ContainsKey("X-Special-Header"),
    specialApp =>
    {
        specialApp.Run(async context =>
        {
            await context.Response.WriteAsync("Special handling for requests with this header");
        });
    });
```

### 5.5 `app.UseWhen()` — Conditional middleware that re-joins the main pipeline

Unlike `MapWhen`, after the conditional branch finishes, the request **continues** through the main pipeline:

```csharp
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    apiApp =>
    {
        apiApp.UseMiddleware<ApiKeyValidationMiddleware>();
    });

// /api/* requests pass through ApiKeyValidationMiddleware and then continue here.
// Non-/api/* requests skip that middleware and go directly here.
app.MapControllers();
```

---

## Section 6 — Important Built-in Middleware

### 6.1 Exception Handling Middleware

**Purpose:** Catch any unhandled exception thrown anywhere in the pipeline and return a clean, safe error response — never exposing internal stack traces to the client.

**Why it must be first:** Because it wraps the entire pipeline, it can only catch exceptions thrown by middleware registered *after* it. If placed in the middle, errors from earlier middleware (e.g. HttpsRedirection) would escape unhandled.

```csharp
// Option 1: Built-in handler (recommended for production)
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        // Force the status code to 500 for all unhandled errors
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        // IExceptionHandlerFeature holds the original exception that was thrown
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                status = 500,
                // NEVER return error.Error.Message or StackTrace to clients in production —
                // it leaks implementation details and can be a security risk.
                message = "An error occurred. Please try again later."
            });
        }
    });
});
```

```csharp
// Option 2: Custom exception middleware (more flexible — lets you handle specific exception types)
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Run the entire rest of the pipeline inside this try block.
            // Any exception thrown by any downstream middleware or action will be caught here.
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            // NotFoundException is a custom domain exception (e.g. "Order #123 not found").
            // Log at Warning level — this is expected and not a system failure.
            _logger.LogWarning(ex, "Resource not found: {Path}", context.Request.Path);
            await WriteErrorResponse(context, 404, ex.Message);
        }
        catch (ValidationException ex)
        {
            // ValidationException represents bad input from the client (400 Bad Request).
            // No need to log — the client sent invalid data, not our fault.
            await WriteErrorResponse(context, 400, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            // The request lacks valid credentials (e.g. expired token, missing token).
            await WriteErrorResponse(context, 401, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            // The user is authenticated but lacks the required permission.
            await WriteErrorResponse(context, 403, ex.Message);
        }
        catch (Exception ex)
        {
            // Catch-all for truly unexpected errors.
            // Log at Error level with full exception details for post-mortem analysis.
            _logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);
            // Return a generic 500 — never expose internal details to the client.
            await WriteErrorResponse(context, 500, "Internal Server Error");
        }
    }

    // Helper to avoid repeating response-writing logic in every catch block
    private static Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new { status = statusCode, message });
    }
}
```

> **Watch out:** You cannot set `StatusCode` or add headers after the response has started writing (`context.Response.HasStarted == true`). If streaming or large responses are involved, always guard with this check before modifying response metadata.

---

### 6.2 Authentication & Authorization Middleware

**Purpose:** Two separate but tightly coupled middlewares that work together.
- `UseAuthentication()` reads the incoming token/cookie, validates it, and populates `context.User` (the `ClaimsPrincipal`).
- `UseAuthorization()` inspects `context.User` and checks whether it satisfies the `[Authorize]` constraints on the matched endpoint.

**Why the order is mandatory:** Authorization checks `context.User`. If Authentication hasn't run yet, `context.User` is unauthenticated — every `[Authorize]` endpoint will return 401, even for valid tokens.

```csharp
// ── Service registration (inside the builder section) ──────────────────────

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,              // Reject tokens not issued by our app
            ValidateAudience = true,            // Reject tokens not intended for our app
            ValidateLifetime = true,            // Reject expired tokens
            ValidateIssuerSigningKey = true,    // Reject tokens with an invalid signature
            ValidIssuer = "your-app",
            ValidAudience = "your-users",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("your-secret-key-min-256-bits"))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Named policies let you express complex rules as a single attribute argument.
    // This policy requires the user to have the "Admin" role claim.
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // This policy requires a specific claim value — useful for fine-grained access control.
    options.AddPolicy("SeniorEmployee", policy =>
        policy.RequireClaim("employee_level", "senior", "lead", "manager"));
});

// ── Middleware pipeline (order is mandatory) ────────────────────────────────

app.UseAuthentication();   // Step 1: parse the token and set context.User
app.UseAuthorization();    // Step 2: enforce [Authorize] constraints on the endpoint
```

Using in a controller:

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    // Any authenticated user can access this endpoint.
    // If no token or invalid token → 401 Unauthorized.
    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile() => Ok(User.Identity?.Name);

    // Only users whose token contains a "role" claim equal to "Admin" can access this.
    // Valid token but wrong role → 403 Forbidden.
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public IActionResult GetAllUsers() => Ok();

    // Uses the named policy defined above.
    // Cleaner than listing roles/claims inline — the policy definition is centralised.
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("users/{id}")]
    public IActionResult DeleteUser(int id) => Ok();
}
```

---

### 6.3 CORS Middleware

**Purpose:** When a browser makes a request from one origin (e.g. `http://localhost:3000`) to a different origin (e.g. `https://api.myapp.com`), the browser sends a CORS preflight (`OPTIONS`) request first. This middleware inspects that preflight and adds the appropriate `Access-Control-*` headers to allow or deny the cross-origin request.

**Why it must come after `UseRouting()`:** The CORS middleware needs to know which endpoint was matched to look up that endpoint's CORS policy (if configured per-endpoint). Without routing resolved first, it falls back to the global policy — which can cause inconsistencies.

```csharp
// ── Service registration ────────────────────────────────────────────────────

builder.Services.AddCors(options =>
{
    // Production policy: only allow requests from known frontend origins.
    // WithOrigins() is an allowlist — anything not in this list will be blocked.
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://myapp.com", "https://www.myapp.com")
              .AllowAnyMethod()       // Allow GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader()       // Allow Content-Type, Authorization, etc.
              .AllowCredentials();    // Required when the browser sends cookies or
                                     // Authorization headers with cross-origin requests.
    });

    // Development policy: allow everything from localhost for local testing.
    // NEVER use in production — it opens the API to any origin.
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()     // Note: AllowAnyOrigin() is incompatible with
              .AllowAnyMethod()     // AllowCredentials() — browser will reject the
              .AllowAnyHeader();    // response if both are set simultaneously.
    });
});

// ── Middleware pipeline ─────────────────────────────────────────────────────

// Apply the policy. Must come after UseRouting() and before UseAuthentication().
app.UseCors("AllowFrontend");
```

---

### 6.4 Static Files Middleware

**Purpose:** Short-circuit the pipeline for requests that map to a physical file in `wwwroot` (or a custom directory), serving the file content directly without ever reaching the router or controller layer. This is very efficient because it avoids all the authentication, routing, and controller overhead for public assets.

**Why it should come early:** Placing it before `UseRouting()` and `UseAuthentication()` means static file requests never go through auth — correct for public assets. If you have *protected* static files, place this after `UseAuthorization()` instead.

```csharp
// Default: serves files from the wwwroot folder at the root URL path.
// A request for GET /images/logo.png → serves wwwroot/images/logo.png.
app.UseStaticFiles();

// Custom: serve files from an "uploads" folder outside wwwroot,
// accessible under the /files URL prefix.
// GET /files/report.pdf → reads from {ContentRootPath}/uploads/report.pdf
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/files"
    // Security note: be careful with this pattern — make sure the uploads folder
    // only contains files that are safe to expose publicly. Never put appsettings.json
    // or private keys in a folder mapped via UseStaticFiles.
});
```

---

### 6.5 Rate Limiting Middleware (ASP.NET Core 7+)

**Purpose:** Protect the API from abuse, DoS attacks, or excessive usage by limiting how many requests a client can make within a time window. When the limit is exceeded, the middleware returns a `429 Too Many Requests` response **without even reaching the controller**, saving server resources.

**Why it comes after `UseAuthorization()`:** Rate limits are often applied per user (authenticated identity). If placed before Authentication, the middleware only has the client's IP address to identify them — placing it after means you can key the limiter on the authenticated user's ID or role.

```csharp
// ── Service registration ────────────────────────────────────────────────────

builder.Services.AddRateLimiter(options =>
{
    // Fixed window limiter: allows up to PermitLimit requests per Window.
    // After the window expires, the counter resets regardless of when in the window
    // the requests arrived. Simple and low-overhead, but can allow bursts at window boundaries.
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;                               // Max 100 requests...
        limiterOptions.Window = TimeSpan.FromMinutes(1);                // ...per minute
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;  // Queue up to 10 extra requests instead of rejecting
    });

    // Sliding window limiter: smoother than fixed window — avoids the burst problem
    // by distributing the quota across segments within the window.
    options.AddSlidingWindowLimiter("sliding", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 6;   // Divide the window into 6 × 10-second segments
        limiterOptions.QueueLimit = 5;
    });

    // The HTTP status code returned when the limit is exceeded.
    // 429 is the standard "Too Many Requests" code.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Middleware pipeline ─────────────────────────────────────────────────────

app.UseRateLimiter();
```

Applying the limiter to specific controllers or endpoints:

```csharp
// Apply the "fixed" policy to all endpoints in this controller.
// Individual action methods can override this with their own [EnableRateLimiting] attribute.
[EnableRateLimiting("fixed")]
[ApiController]
[Route("api/[controller]")]
public class PublicApiController : ControllerBase
{
    // Exempt this specific endpoint from rate limiting entirely
    // (e.g. a lightweight health check that must always be reachable).
    [DisableRateLimiting]
    [HttpGet("health")]
    public IActionResult Health() => Ok();

    // Uses the controller-level "fixed" policy
    [HttpGet("data")]
    public IActionResult GetData() => Ok();
}
```

---

### 6.6 Response Compression Middleware

**Purpose:** Transparently compress response bodies (using Brotli or Gzip) before sending them to the client. This reduces bandwidth consumption and improves perceived performance, especially for large JSON payloads or HTML pages.

**Why it should come early in the pipeline:** Placing it near the top means the entire response — including error responses from ExceptionHandler — gets compressed. If placed after StaticFiles, static assets won't benefit from compression (they should be pre-compressed or served via a CDN anyway, but this matters for API responses).

```csharp
// ── Service registration ────────────────────────────────────────────────────

builder.Services.AddResponseCompression(options =>
{
    // EnableForHttps: compression over HTTPS is slightly less secure due to CRIME/BREACH attacks,
    // but the risk is minimal for typical APIs. Enable it in most cases.
    options.EnableForHttps = true;

    // Brotli is more efficient than Gzip (20-30% better compression) but older clients
    // may not support it. ASP.NET Core negotiates the best algorithm automatically
    // based on the client's Accept-Encoding header.
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();

    // By default, only certain MIME types are compressed.
    // Add any custom types your API returns.
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/plain" });
});

// ── Middleware pipeline ─────────────────────────────────────────────────────

// Must come BEFORE StaticFiles and MapControllers so that those responses get compressed.
app.UseResponseCompression();
```

---

### 6.7 Response Caching Middleware

**Purpose:** Cache complete HTTP responses in memory (or a distributed cache) and serve subsequent identical requests from the cache — bypassing all downstream middleware, authentication, routing, and controller logic entirely. Ideal for public, read-only endpoints whose data changes infrequently.

**Why it comes just before `MapControllers()`:** The middleware needs to know the route (from Routing) and the authenticated user (from Authentication) to generate a correct cache key. Placing it too early would result in wrong cache hits (e.g. returning one user's data to another user).

```csharp
// ── Service registration ────────────────────────────────────────────────────

builder.Services.AddResponseCaching();

// ── Middleware pipeline ─────────────────────────────────────────────────────

// Must come after UseRouting() and UseAuthentication(), just before MapControllers().
app.UseResponseCaching();
```

Applying caching to specific actions:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Cache this response for 60 seconds, shared by all clients (CDN-friendly).
    // The middleware adds Cache-Control: public, max-age=60 headers automatically.
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [HttpGet]
    public IActionResult GetProducts() => Ok(_products);

    // Cache per user: each authenticated user gets their own cache entry.
    // Uses the Vary-By-User feature — requires configuring a VaryByCustom profile.
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
    [HttpGet("recommendations")]
    public IActionResult GetRecommendations() => Ok();

    // Never cache this endpoint — always return fresh data.
    [ResponseCache(NoStore = true)]
    [HttpGet("{id}")]
    public IActionResult GetProduct(int id) => Ok();
}
```

---

## Section 7 — Real-world Custom Middleware

### 7.1 Request/Response Logging with Correlation ID

**What it does:** Logs every incoming request and outgoing response, including method, path, status code, and duration. It also attaches a **Correlation ID** — a short unique string — to each request. This ID is echoed back in the response header and included in every log line, so when debugging distributed systems or reading logs, you can filter all log entries for a single request by its ID.

```csharp
public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;

    public HttpLoggingMiddleware(RequestDelegate next,
        ILogger<HttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        // Try to read an existing Correlation ID from the request header.
        // This allows upstream services (API gateway, load balancer) to pass their own ID.
        // If none is provided, generate a short one from a new GUID.
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N")[..8];

        // Echo the Correlation ID back in the response header so the client can include it
        // in bug reports or support tickets.
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // BeginScope attaches the Correlation ID to all log entries written within this scope,
        // even from deep inside controllers and services — no need to pass it around manually.
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        _logger.LogInformation(
            "[{Id}] → {Method} {Path}{Query} from {IP}",
            correlationId,
            request.Method,
            request.Path,
            request.QueryString,
            context.Connection.RemoteIpAddress);

        var sw = Stopwatch.StartNew();

        // Run the rest of the pipeline (routing, auth, controller, etc.)
        await _next(context);

        sw.Stop();

        // Choose log level based on the response status code:
        // 5xx → Error (system failure), 4xx → Warning (client error), 2xx/3xx → Information
        var level = context.Response.StatusCode >= 500
            ? LogLevel.Error
            : context.Response.StatusCode >= 400
                ? LogLevel.Warning
                : LogLevel.Information;

        _logger.Log(level,
            "[{Id}] ← {StatusCode} ({Ms}ms)",
            correlationId,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
```

---

### 7.2 API Key Validation Middleware

**What it does:** Protects a set of endpoints (typically a public API) by requiring callers to include a pre-shared secret key in the `X-API-Key` request header. This is simpler than full JWT authentication and is commonly used for machine-to-machine or third-party integrations where users don't have individual accounts.

The middleware short-circuits the pipeline if the key is missing or invalid — the request never reaches routing, the controller, or the database.

```csharp
public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check whether the header is present at all.
        // If not, return 401 — the caller hasn't identified themselves.
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "API Key missing" });
            return;  // Short-circuit: do NOT call _next — the pipeline stops here.
        }

        // Compare the provided key against the configured secret using Ordinal comparison
        // (case-sensitive, culture-insensitive) to avoid subtle string comparison bugs.
        var validApiKey = _config["ApiKey"];
        if (!string.Equals(extractedApiKey, validApiKey, StringComparison.Ordinal))
        {
            // The header is present but the value doesn't match — 403 Forbidden.
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid API Key" });
            return;
        }

        // Key is valid — continue to the next middleware.
        await _next(context);
    }
}
```

---

### 7.3 Request Body Buffering Middleware

**What it does:** By default, `Request.Body` is a **forward-only stream** — once read, it cannot be rewound. This becomes a problem when multiple middlewares (or a middleware + a controller) each need to read the body. For example, a logging middleware reads the body to log its content, then the model binder in the controller tries to read it again and finds an empty stream.

This middleware calls `EnableBuffering()` which swaps the default stream with a rewindable `MemoryStream` or `FileBufferingReadStream` (for large bodies), allowing the body to be read multiple times.

```csharp
public class RequestBodyBufferingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestBodyBufferingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // EnableBuffering() replaces Request.Body with a buffered, seekable stream.
        // For bodies under 30KB (default threshold), the buffer lives in memory.
        // For larger bodies, it spills to a temp file on disk automatically.
        // This must be called BEFORE any downstream code reads the body.
        context.Request.EnableBuffering();

        await _next(context);
        // No cleanup needed — the framework disposes the buffered stream at end of request.
    }
}
```

Reading the body in a downstream middleware after buffering is enabled:

```csharp
// Reset the stream position to the beginning before each read.
// Without this, reads after the first one return empty content.
context.Request.Body.Position = 0;

using var reader = new StreamReader(
    context.Request.Body,
    leaveOpen: true);           // leaveOpen: true — do not close the stream after reading,
                                // so downstream code (e.g. the model binder) can still read it.
var body = await reader.ReadToEndAsync();

// ALWAYS reset position after reading so the next reader starts from the beginning.
context.Request.Body.Position = 0;
```

---

### 7.4 Maintenance Mode Middleware

**What it does:** When maintenance mode is enabled via configuration, this middleware intercepts every request and immediately returns `503 Service Unavailable` with a `Retry-After` header — before the request reaches any controller, database, or business logic. This protects a partially-upgraded or offline system from receiving traffic.

A bypass list (configured as an array of trusted IP addresses) allows the development team to access the system during maintenance for testing and verification, without disabling the block for everyone else.

```csharp
public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public MaintenanceModeMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Read the flag from appsettings.json or environment variables.
        // Setting "MaintenanceMode": true in config enables the block without redeployment.
        var isMaintenanceMode = _config.GetValue<bool>("MaintenanceMode");

        if (isMaintenanceMode)
        {
            // Load the bypass IP list from config — typically the dev team's office IPs.
            // If the section doesn't exist or is empty, no one is bypassed.
            var allowedIps = _config.GetSection("MaintenanceBypassIPs")
                                    .Get<string[]>() ?? Array.Empty<string>();

            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            if (!allowedIps.Contains(clientIp))
            {
                // 503 Service Unavailable — the standard code for "we're down on purpose".
                context.Response.StatusCode = 503;

                // Retry-After tells clients and crawlers how many seconds to wait
                // before retrying. This prevents hammering the server and helps
                // search engines not penalise your site for downtime.
                context.Response.Headers["Retry-After"] = "3600";

                await context.Response.WriteAsJsonAsync(new
                {
                    status = 503,
                    message = "System is under maintenance. Please try again later."
                });
                return; // Short-circuit — bypassed IPs fall through to _next below.
            }
        }

        await _next(context);
    }
}
```

---

## Section 8 — Full Production Pipeline Overview

```
HTTP Request
      │
      ▼
┌─────────────────────────────┐
│  ExceptionHandler            │  ← catches all unhandled errors
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  HttpsRedirection            │  ← redirects HTTP → HTTPS
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  ResponseCompression         │  ← compresses response (Brotli/Gzip)
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  RequestLogging              │  ← logs request/response, attaches Correlation ID
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  StaticFiles                 │  ← serves static files immediately if matched
└──────────┬──────────────────┘
           ▼ (not a static file)
┌─────────────────────────────┐
│  Routing                     │  ← identifies the matching endpoint
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  Cors                        │  ← validates the request origin
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  Authentication              │  ← parses the token, populates context.User
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  Authorization               │  ← checks access permissions
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  RateLimiter                 │  ← throttles excessive requests (ASP.NET Core 7+)
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  ResponseCaching             │  ← returns cached response if available
└──────────┬──────────────────┘
           ▼
┌─────────────────────────────┐
│  MapControllers              │  ← invokes the action method
└──────────┬──────────────────┘
           ▼
      Action Method
      (your business logic)
           │
           ▼  Response travels back through each middleware in reverse order
```

---

## Section 9 — Inspecting & Debugging the Pipeline

### 9.1 Developer Exception Page

```csharp
if (app.Environment.IsDevelopment())
{
    // In development: show a full exception page with stack trace, request details,
    // and route information directly in the browser. Never use in production.
    app.UseDeveloperExceptionPage();
}
else
{
    // In production: catch the exception silently and show a safe, generic error page.
    app.UseExceptionHandler("/error");
}
```

### 9.2 Simple checkpoint middleware for debugging

Insert this anywhere in the pipeline to confirm execution order and verify a request is reaching that point:

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine($"[DEBUG] → Checkpoint reached: {context.Request.Path}");
    await next(context);
    Console.WriteLine($"[DEBUG] ← Response status: {context.Response.StatusCode}");
});
```

### 9.3 Checking `context.Response.HasStarted`

Once the response body has begun writing (headers have been flushed to the network), it is **impossible** to change the status code or add/modify headers. Attempting to do so throws an `InvalidOperationException`. Always guard against this in error-handling middleware:

```csharp
if (!context.Response.HasStarted)
{
    // Safe to modify headers and status code here
    context.Response.StatusCode = 500;
    context.Response.Headers["X-Error"] = "true";
}
else
{
    // Too late — the response is already on the wire.
    // At this point you can only log the problem; you cannot change what the client receives.
    _logger.LogWarning("Cannot modify response — it has already started for {Path}",
        context.Request.Path);
}
```

---

## Summary

```
Pipeline = a chain of Middleware that processes request/response in order
         = immutable after the application starts

Middleware registration variants:
  ├── app.Use()      → pass-through (calls next)
  ├── app.Run()      → terminal (no next, stops the pipeline)
  ├── app.Map()      → branch by URL path
  ├── app.MapWhen()  → branch by arbitrary condition
  └── app.UseWhen()  → conditional middleware that re-joins the main pipeline

Recommended order:
  ExceptionHandler → HttpsRedirection → ResponseCompression
  → StaticFiles → Routing → Cors
  → Authentication → Authorization → RateLimiter
  → ResponseCaching → MapControllers

Writing custom middleware:
  ├── Inline:  app.Use(async (ctx, next) => { ... })
  └── Class:   implement InvokeAsync(HttpContext context)
               inject Scoped services as InvokeAsync parameters, NOT in the constructor

Common mistakes:
  ├── Always 401          → UseAuthorization() placed before UseAuthentication()
  ├── CORS errors         → UseCors() placed before UseRouting()
  └── Cannot set headers  → Response.HasStarted is already true
```

---
