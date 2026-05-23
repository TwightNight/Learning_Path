# Error Handling & Response Standardization in ASP.NET Core

---

## Part 1 — The Problem Without Standardization

Consider an API with no response standardization:

```json
// Endpoint 1 returns errors like this
{ "message": "Not found" }

// Endpoint 2 returns errors differently
{ "error": "Validation failed", "fields": ["name"] }

// Endpoint 3 — ASP.NET Core default exception response
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "traceId": "00-abc123..."
}

// Endpoint 4 — even success responses differ
[{ "id": 1, "name": "Laptop" }]          // this endpoint returns a raw array
{ "data": { "id": 1, "name": "Laptop" }} // this one wraps it in a data object
```

The client (frontend/mobile) must handle 4 different response shapes — very hard to maintain. The goal is for **every response to have a consistent shape**.

---

## Part 2 — ProblemDetails: The RFC 7807 Standard

**ProblemDetails** is an international standard (RFC 7807) for describing errors in HTTP APIs. ASP.NET Core has built-in support — no extra package needed.

Standard structure:

```json
{
  "type": "https://example.com/errors/validation-failed",
  "title": "Invalid Data",
  "status": 400,
  "detail": "The 'email' field is not in a valid format",
  "instance": "/api/users",
  "traceId": "00-abc123def456-01"
}
```

| Field | Meaning |
|-------|---------|
| `type` | URI describing the error type (can be a docs link) |
| `title` | Short error name, consistent across occurrences |
| `status` | HTTP status code |
| `detail` | Specific description of this particular error |
| `instance` | URI of the request that caused the error |
| `traceId` | ID for log tracing (automatically added by ASP.NET Core) |

Enable ProblemDetails in `Program.cs`:

```csharp
builder.Services.AddProblemDetails();
```

---

## Part 3 — Custom Exception Classes

Before building error handling, define custom exceptions to distinguish error types:

```csharp
// Base exception for the entire app
public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }   // error code for client-side automation

    protected AppException(
        string message,
        int statusCode,
        string errorCode) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode  = errorCode;
    }
}

// 404 — resource not found
public class NotFoundException : AppException
{
    public NotFoundException(string resource, object id)
        : base($"{resource} with ID '{id}' does not exist", 404, "NOT_FOUND")
    { }
}

// 400 — invalid data (business logic error, distinct from validation)
public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, 400, "BAD_REQUEST")
    { }
}

// 409 — data conflict
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, 409, "CONFLICT")
    { }
}

// 403 — insufficient permissions
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action")
        : base(message, 403, "FORBIDDEN")
    { }
}

// 422 — business logic validation (distinct from 400)
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Invalid data", 422, "VALIDATION_FAILED")
    {
        Errors = errors;
    }
}
```

Usage in a service:

```csharp
public async Task<Order> GetByIdAsync(int id)
{
    var order = await _db.Orders.FindAsync(id);
    if (order == null)
        throw new NotFoundException("Order", id);  // ← clear and consistent

    return order;
}

public async Task<User> RegisterAsync(RegisterDto dto)
{
    var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
    if (exists)
        throw new ConflictException("Email is already in use");

    // ...
}
```

---

## Part 4 — Global Exception Middleware

A single middleware to handle **all** uncaught exceptions — instead of try/catch in every controller:

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Determine status code and error code by exception type
        var (statusCode, errorCode, message) = ex switch
        {
            AppException appEx => (appEx.StatusCode, appEx.ErrorCode, appEx.Message),
            _ => (500, "INTERNAL_ERROR", "An error occurred, please try again later")
        };

        // Log details — but do NOT expose to the client in production
        if (statusCode >= 500)
            _logger.LogError(ex,
                "Unhandled exception: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        else
            _logger.LogWarning(ex,
                "Handled exception [{Code}]: {Message}",
                errorCode, ex.Message);

        // Build ProblemDetails response
        var problem = new ProblemDetails
        {
            Type     = $"https://myapp.com/errors/{errorCode.ToLower()}",
            Title    = GetTitle(statusCode),
            Status   = statusCode,
            Detail   = message,
            Instance = context.Request.Path
        };

        // Add detailed errors if it's a ValidationException
        if (ex is ValidationException validationEx)
            problem.Extensions["errors"] = validationEx.Errors;

        // Add traceId for log tracing
        problem.Extensions["traceId"] =
            context.TraceIdentifier;

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        _   => "Internal Server Error"
    };
}

// Extension method
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
```

Register it **first** in the pipeline:

```csharp
// Program.cs
app.UseGlobalExceptionHandler();   // ← must be first
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## Part 5 — Standardizing Validation Errors (400)

By default, when `[ApiController]` catches a validation error, the response format differs slightly from the custom ProblemDetails format. Override it for consistency:

```csharp
// Program.cs — after AddControllers()
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Aggregate all errors by field
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray()
            );

        var problem = new ProblemDetails
        {
            Type     = "https://myapp.com/errors/validation-failed",
            Title    = "Invalid Data",
            Status   = 400,
            Detail   = "One or more fields are invalid",
            Instance = context.HttpContext.Request.Path
        };

        problem.Extensions["errors"]  = errors;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});
```

Validation error responses are now consistent:

```json
{
  "type": "https://myapp.com/errors/validation-failed",
  "title": "Invalid Data",
  "status": 400,
  "detail": "One or more fields are invalid",
  "instance": "/api/products",
  "traceId": "00-abc123-01",
  "errors": {
    "name": ["Product name is required"],
    "price": ["Price must be greater than 0", "Price must be a number"]
  }
}
```

---

## Part 6 — Response Envelope: Standardizing Success Responses

Beyond errors, success responses should also be consistent. A **Response Envelope** wraps every response into the same shape:

```json
// Instead of returning raw data
{ "id": 1, "name": "Laptop" }

// Wrap it in a consistent envelope
{
  "success": true,
  "data": { "id": 1, "name": "Laptop" },
  "message": null,
  "meta": null
}
```

### 6.1 Define the Envelope Class

```csharp
// Single-item response
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public PaginationMeta? Meta { get; init; }

    // Factory methods — create responses quickly and consistently
    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Created(T data)
        => new() { Success = true, Data = data, Message = "Created successfully" };

    public static ApiResponse<T> NoData(string message)
        => new() { Success = true, Data = default, Message = message };
}

// Metadata for pagination
public class PaginationMeta
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}

// Paginated list response
public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public static PagedResponse<T> Ok(
        IEnumerable<T> data,
        int page,
        int pageSize,
        int totalItems)
        => new()
        {
            Success = true,
            Data    = data,
            Meta    = new PaginationMeta
            {
                Page       = page,
                PageSize   = pageSize,
                TotalItems = totalItems
            }
        };
}
```

### 6.2 Usage in a Controller

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
        => _service = service;

    // GET /api/products?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, total) = await _service.GetPagedAsync(page, pageSize);

        return Ok(PagedResponse<ProductDto>.Ok(items, page, pageSize, total));
    }

    // GET /api/products/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetByIdAsync(id);
        // NotFoundException is automatically caught by GlobalExceptionMiddleware
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    // POST /api/products
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = await _service.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ApiResponse<ProductDto>.Created(product));
    }

    // DELETE /api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<object>.NoData("Deleted successfully"));
    }
}
```

Actual responses received by the client:

```json
// GET /api/products?page=1&pageSize=2
{
  "success": true,
  "data": [
    { "id": 1, "name": "Laptop", "price": 999 },
    { "id": 2, "name": "Mouse",  "price": 29  }
  ],
  "message": null,
  "meta": {
    "page": 1,
    "pageSize": 2,
    "totalItems": 50,
    "totalPages": 25,
    "hasNext": true,
    "hasPrev": false
  }
}

// POST /api/products — 201 Created
{
  "success": true,
  "data": { "id": 51, "name": "Keyboard", "price": 79 },
  "message": "Created successfully",
  "meta": null
}

// DELETE /api/products/5 — 200 OK
{
  "success": true,
  "data": null,
  "message": "Deleted successfully",
  "meta": null
}
```

---

## Part 7 — Full Error & Response Flow

```
Incoming Request
      │
      ▼
[GlobalExceptionMiddleware]  ← outermost layer, catches all exceptions
      │
      ▼
[Validation — ApiController]
  ├── Error → InvalidModelStateResponseFactory
  │            → ProblemDetails 400 + errors{}
  └── OK   → continue
      │
      ▼
Action Method
  ├── throw NotFoundException   ─┐
  ├── throw ConflictException    ├→ GlobalExceptionMiddleware
  ├── throw ForbiddenException   │   → ProblemDetails with correct status
  └── throw Exception (bug)     ─┘   → ProblemDetails 500

  └── Success
        → ApiResponse<T>.Ok(data)
        → PagedResponse<T>.Ok(data, pagination)
```

---

## Part 8 — All Response Shapes at a Glance

```json
// ✅ Success — single item
{
  "success": true,
  "data": { },
  "message": null,
  "meta": null
}

// ✅ Success — paginated list
{
  "success": true,
  "data": [ ],
  "message": null,
  "meta": { "page": 1, "pageSize": 10, "totalItems": 100, "totalPages": 10, "hasNext": true, "hasPrev": false }
}

// ❌ Validation error — 400
{
  "type": "https://myapp.com/errors/validation-failed",
  "title": "Invalid Data",
  "status": 400,
  "detail": "One or more fields are invalid",
  "instance": "/api/products",
  "traceId": "00-abc123-01",
  "errors": { "name": ["Required"], "price": ["Must be > 0"] }
}

// ❌ Not found — 404
{
  "type": "https://myapp.com/errors/not_found",
  "title": "Not Found",
  "status": 404,
  "detail": "Order with ID '99' does not exist",
  "instance": "/api/orders/99",
  "traceId": "00-def456-01"
}

// ❌ Server error — 500
{
  "type": "https://myapp.com/errors/internal_error",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred, please try again later",
  "instance": "/api/orders",
  "traceId": "00-ghi789-01"
}
```

---

## Summary

```
Error Handling & Response Standardization

├── Custom Exceptions
│     AppException (base) → NotFoundException, ConflictException...
│     Thrown from service → caught centrally in middleware

├── GlobalExceptionMiddleware
│     Catches all exceptions → maps to ProblemDetails with correct status
│     Logs 5xx as Error, 4xx as Warning

├── ProblemDetails (RFC 7807)
│     International standard for error responses
│     type / title / status / detail / instance / traceId / errors

├── InvalidModelStateResponseFactory
│     Overrides [ApiController] validation error format
│     → same ProblemDetails format

└── Response Envelope
      ApiResponse<T>        → single-item response
      PagedResponse<T>      → list + pagination meta
      Consistent shape: success / data / message / meta
```
