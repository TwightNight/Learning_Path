


Here is the complete, professional English translation of the ultimate guide. I have preserved the formatting, structure, and real-world tips to ensure it remains a top-tier reference for production-grade API design.

--- START OF FILE ---

# THE COMPREHENSIVE GUIDE: ROUTE DESIGN IN ASP.NET CORE WEB API

> From basics to advanced — The ultimate playbook for production-ready API design

---

## Table of Contents

1. [What is a Route?](#part-1--what-is-a-route)
2. [Conventional vs Attribute Routing](#part-2--conventional-vs-attribute-routing)
3. [Route Templates & Constraints](#part-3--route-templates--constraints)
4. [Tokens & Route Overriding](#part-4--tokens--route-overriding)
5. [Data Binding & Explicit Binding](#part-5--data-binding--explicit-binding)
6. [Resolving Route Conflicts](#part-6--resolving-route-conflicts-ambiguousmatchexception)
7. [Nested Routes](#part-7--nested-routes)
8. [API Versioning via Routes](#part-8--api-versioning-via-routes)
9. [RESTful API Design (Full CRUD Example)](#part-9--restful-api-design-full-crud-example)
10. [Minimal APIs (.NET 6+)](#part-10--minimal-apis-net-6-comparison)
11. [Anti-patterns & Common Mistakes](#part-11--anti-patterns--common-mistakes)
12. [Summary Diagram & Checklist](#summary-diagram--checklist)

---

## Part 1 — What is a Route?

A **Route** is a rule that maps a specific incoming URL to a corresponding piece of code (handler).

When a client sends a request like `GET /api/products/5`, ASP.NET Core needs to determine: *"Which method in my code should handle this request?"* — This is the job of **Routing**.

### The Routing Flow

```text
Client Request
     │
     ▼
┌─────────────────────────────────────┐
│         Routing Middleware          │
│                                     │
│  Step 1: Route Matching             │
│  → Find a route template that       │
│    matches the incoming URL         │
│                                     │
│  Step 2: Route Execution            │
│  → Invoke the matching Action       │
└─────────────────────────────────────┘
     │
     ▼
Controller Action Method
```

### Routing Middleware Pipeline

In `Program.cs`, two crucial middlewares must be placed in the correct order:

```csharp
app.UseRouting();        // Step 1: Matching — determine which endpoint matches
// ... auth middleware, cors, etc.
app.UseEndpoints(...);   // Step 2: Execution — execute the matched endpoint

// Or more concisely in .NET 6+:
app.MapControllers();    // Combines both steps automatically
```

> **Note:** If you are using `[ApiController]` with `app.MapControllers()`, the two steps above are implicitly combined and configured for you.

---

## Part 2 — Conventional vs Attribute Routing

ASP.NET Core supports two routing strategies:

### 2.1 Conventional Routing (Centralized Configuration)

All routes are defined in a **single centralized location** within `Program.cs`. This is suitable for traditional MVC applications returning HTML Views, but it is **highly discouraged** for Web APIs.

```csharp
// Program.cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Drawbacks for Web APIs:**
- You cannot see the API's URL just by looking at the Controller code.
- High risk of confusion in large projects.
- Hard to apply fine-grained control over individual endpoints.

### 2.2 Attribute Routing ✅ The Standard for Web APIs

Routes are declared directly using attributes right above the Controller and its Actions:

```csharp
[ApiController]
[Route("api/products")]          // ← Base route for the entire controller
public class ProductsController : ControllerBase
{
    [HttpGet]                    // Matches: GET /api/products
    public IActionResult GetAll() { ... }

    [HttpGet("{id}")]            // Matches: GET /api/products/5
    public IActionResult GetById(int id) { ... }

    [HttpPost]                   // Matches: POST /api/products
    public IActionResult Create([FromBody] CreateProductDto dto) { ... }

    [HttpPut("{id}")]            // Matches: PUT /api/products/5
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto) { ... }

    [HttpDelete("{id}")]         // Matches: DELETE /api/products/5
    public IActionResult Delete(int id) { ... }
}
```

**Pros:**
- Look at the code, instantly know the URL (no cross-referencing needed).
- Easy to control each endpoint individually.
- Fully supports HTTP verbs: GET, POST, PUT, PATCH, DELETE.

---

## Part 3 — Route Templates & Constraints

A route template is a string pattern defining the structure of the URL. This is the heart of Routing.

### 3.1 Literal Segments

```csharp[Route("api/products")]
// URL MUST strictly contain "api/products"
// ✅ /api/products       → Matches
// ❌ /api/product        → Fails
// ❌ /Api/Products       → Fails (routing is default case-insensitive, but consistency is key)
```

### 3.2 Route Parameters (Dynamic Segments)

Use curly braces `{}` to declare parameters. ASP.NET Core automatically **extracts** the value from the URL and binds it to the method parameter:

```csharp
[HttpGet("{id}")]
public IActionResult GetById(int id)
// GET /api/products/99  →  id = 99
// GET /api/products/abc →  id = 0 (or returns 404 if a constraint is applied)

// Multiple parameters:[HttpGet("{year}/{month}/{day}")]
public IActionResult GetByDate(int year, int month, int day)
// GET /api/logs/2026/05/11  →  year=2026, month=5, day=11
```

### 3.3 Catch-all Parameters 🔥

If your parameter contains slashes (`/`), standard routing will fail because it treats them as separate segments. Use `*` or `**` to catch everything:

```csharp
// Using * (single wildcard)
[HttpGet("files/{*filePath}")]
public IActionResult GetFile(string filePath)
// GET /api/files/documents/2026/report.pdf
// → filePath = "documents/2026/report.pdf"

// Using ** (double wildcard — preserves URL encoding)
[HttpGet("proxy/{**remainingPath}")]
public IActionResult Proxy(string remainingPath)
// GET /api/proxy/external/service/endpoint?query=1
// → remainingPath = "external/service/endpoint"
```

> **When to use `**` instead of `*`?**
> Use `**` when you need to forward the request exactly as-is to another service (Reverse Proxy pattern), as it retains URL-encoded characters.

### 3.4 Optional Parameters & Default Values

```csharp
// Optional — use a question mark (?)[HttpGet("{id?}")]
public IActionResult GetById(int? id)
// GET /api/products     → id = null
// GET /api/products/5   → id = 5

// Default value — used if the client doesn't provide it
[HttpGet("page/{page=1}/size/{size=10}")]
public IActionResult GetPage(int page, int size)
// GET /api/products/page/2/size/20  → page=2, size=20
// GET /api/products/page            → page=1 (default), size=10 (default)

// Combined with Query Strings (the preferred approach for pagination):
[HttpGet]
public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
// GET /api/products?page=2&size=20  → page=2, size=20
// GET /api/products                 → page=1, size=10
```

### 3.5 Route Constraints

Ensure the parameter is in the correct format **before** hitting your code. If it doesn't match → ASP.NET Core automatically returns `404 Not Found`.

```csharp
// Syntax: {paramName:constraint}
[HttpGet("{id:int}")]          // Only accepts integers
[HttpGet("{id:guid}")]         // Only accepts valid GUIDs
[HttpGet("{slug:alpha}")]      // Only accepts alphabetical strings[HttpGet("{code:length(3,5)}")] // String length between 3 and 5 characters
```

**Common Constraints Cheat Sheet:**

| Constraint | Meaning | Valid URL Example |
|---|---|---|
| `:int` | 32-bit Integer | `/products/5` |
| `:long` | 64-bit Integer | `/items/9999999999` |
| `:double` | Floating-point | `/price/9.99` |
| `:bool` | true/false | `/toggle/true` |
| `:guid` | Valid GUID | `/users/3fa85f64-...` |
| `:datetime` | Date & Time | `/logs/2026-05-11` |
| `:alpha` | Alphabetical only | `/category/laptop` |
| `:minlength(n)` | Minimum length | `/code/AB` (n=2) |
| `:maxlength(n)` | Maximum length | `/tag/hello` (n=10) |
| `:length(n,m)` | Length range | `/zip/70000` |
| `:min(n)` | Minimum value | `/age/18` (n=18) |
| `:max(n)` | Maximum value | `/page/100` (n=100) |
| `:range(n,m)` | Value range | `/rating/4` (1-5) |
| `:regex(pattern)` | Regex match | `/code/[A-Z]{3}` |

**Chaining multiple constraints:**

```csharp
// 'id' must be an integer AND >= 1
[HttpGet("{id:int:min(1)}")]
public IActionResult GetById(int id) { ... }

// 'productCode' must be alphabetical AND exactly 3 characters long
[HttpGet("{productCode:alpha:length(3)}")]
public IActionResult GetByCode(string productCode) { ... }
```

### 3.6 Route Names 🔥

Naming a route allows you to dynamically generate URLs pointing to it from anywhere in your code — especially useful when returning `201 Created` responses:

```csharp
// Name the route using the 'Name' property
[HttpGet("{id:int}", Name = "GetProductById")]
public IActionResult GetById([FromRoute] int id)
{
    var product = _service.GetById(id);
    if (product == null) return NotFound();
    return Ok(product);
}

[HttpPost]
public IActionResult Create([FromBody] CreateProductDto dto)
{
    var newProduct = _service.Create(dto);
    
    // CreatedAtRoute does 3 things automatically:
    // 1. Returns an HTTP 201 Created status
    // 2. Adds a Location Header (e.g., Location: /api/products/100)
    // 3. Returns the newly created object in the body
    return CreatedAtRoute(
        routeName: "GetProductById",
        routeValues: new { id = newProduct.Id },
        value: newProduct
    );
}
```

---

## Part 4 — Tokens & Route Overriding

### 4.1 Tokens (`[controller]` and `[action]`)

Instead of hardcoding the controller name, use a token so the route updates automatically if you rename the class:

```csharp
[Route("api/[controller]")]
public class ProductsController : ControllerBase
// → Automatically resolves to: /api/products
// (ASP.NET Core drops the "Controller" suffix and lowercases it)

[Route("api/[controller]")]
public class ShoppingCartController : ControllerBase
// → /api/shoppingcart

// The [action] token — HIGHLY DISCOURAGED for Web APIs:
[Route("api/[controller]/[action]")]
// → /api/products/getall (URL contains verbs — violates REST principles)
```

> **Why avoid `[action]` in Web APIs?**
> RESTful APIs are designed around **resources** (nouns), not **actions** (verbs). The URL `/api/products` combined with the HTTP verb `GET` provides enough semantic meaning. You don't need `/api/products/getall`.

### 4.2 Route Overriding (Absolute Routes) 🔥

By default, an Action's route is **appended** to the Controller's Base Route. If you want an Action to have a **completely independent** path, start the route template with a forward slash `/`:

```csharp
[ApiController][Route("api/[controller]")]       // Base: /api/products
public class ProductsController : ControllerBase
{
    [HttpGet]                     // URL: /api/products ✅
    public IActionResult GetAll() { ... }[HttpGet("{id:int}")]         // URL: /api/products/5 ✅
    public IActionResult GetById(int id) { ... }

    // The "/" breaks the concatenation rule
    [HttpGet("/health")]          // URL: /health (NOT /api/products/health)
    public IActionResult HealthCheck() { return Ok(new { status = "healthy" }); }

    [HttpGet("/ping")]            // URL: /ping
    public IActionResult Ping() { return Ok("Pong"); }
}
```

**Real-world use case:** Endpoints like `/health`, `/ping`, or `/metrics` are often placed at the root level so monitoring tools can easily access them without the `api/` prefix.

---

## Part 5 — Data Binding & Explicit Binding

ASP.NET Core needs to know **where to look** in the HTTP request to find values for your method parameters.

### 5.1 Data Sources in an HTTP Request

```text
HTTP Request
├── Route path: /api/products/5          → [FromRoute]
├── Query str:  ?sort=price&page=2       → [FromQuery]
├── Body JSON:  { "name": "Laptop" }     → [FromBody]
├── Headers:    Accept-Language: en-US   →[FromHeader]
└── Form Data:  multipart/form-data      → [FromForm]
```

### 5.2 Route Parameter vs Query String — When to use what?

| | Route Parameter | Query String |
|---|---|---|
| **Purpose** | Identify a specific resource | Filter, sort, or paginate a collection |
| **URL Syntax** | `/api/products/5` | `/api/products?sort=price&page=2` |
| **Required?** | Usually required | Usually optional |
| **Examples** | `GET /users/42` | `GET /users?role=admin&active=true` |

```csharp
// ✅ Correct — Using Route Param to identify a resource
GET /api/orders/10

// ✅ Correct — Using Query String to filter a collection
GET /api/orders?status=pending&from=2026-01-01

// ❌ Incorrect — Using Query String to identify a specific resource
GET /api/orders?id=10
```

### 5.3 Explicit Binding — Best Practice 🔥

While ASP.NET Core is smart enough to guess where data comes from, **always explicitly declare it** to prevent security flaws and make the code readable:

```csharp
[HttpPut("{id:int}")]
public IActionResult Update(
    [FromRoute] int id,                                    // From URL path
    [FromBody] UpdateProductDto dto,                       // From JSON body
    [FromQuery] string? auditReason,                       // From query string[FromHeader(Name = "Accept-Language")] string? lang,   // From HTTP header[FromHeader(Name = "X-Correlation-ID")] string? correlationId)
{
    // id = 5 (from /api/products/5)
    // dto = { name: "New Name" } (from request body)
    // auditReason = "price correction" (from ?auditReason=price+correction)
    // lang = "en-US" (from header Accept-Language: en-US)
    ...
}
```

### 5.4 `[FromBody]` — Crucial Facts

```csharp
// An Action can only have ONE[FromBody] parameter!
// ❌ Incorrect — 2 [FromBody] in the same action
public IActionResult Update([FromBody] UpdateDto dto, [FromBody] MetaDto meta) { }

// ✅ Correct — Wrap them into a single object
public IActionResult Update([FromBody] UpdateRequestDto request)
// Where UpdateRequestDto contains both data and meta objects

// [ApiController] automatically applies [FromBody] to complex types.
// However, explicitly declaring it is still a best practice.
```

### 5.5 `[FromForm]` — File Uploads

```csharp
[HttpPost("upload")]
public async Task<IActionResult> Upload(
    [FromForm] string description,
    [FromForm] IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("No file uploaded");

    var fileName = Path.GetFileName(file.FileName);
    var savePath = Path.Combine("uploads", fileName);
    
    using var stream = new FileStream(savePath, FileMode.Create);
    await file.CopyToAsync(stream);
    
    return Ok(new { fileName, size = file.Length });
}
```

---

## Part 6 — Resolving Route Conflicts (AmbiguousMatchException) 🔥

### The Problem

When two routes can match the exact same URL, ASP.NET Core crashes and throws an `AmbiguousMatchException`:

```csharp
// ❌ Conflict — /api/products/top matches both!
[HttpGet("top")]                    // Route (1): "top" is a literal string
public IActionResult GetTopProducts() { ... }[HttpGet("{category}")]             // Route (2): "top" could be a value for {category}
public IActionResult GetByCategory(string category) { ... }
```

### Solution 1: Use the `Order` property

```csharp
[HttpGet("top", Order = 1)]         // Higher priority (lower number runs first)
public IActionResult GetTopProducts() { ... }

[HttpGet("{category}", Order = 2)]  // Only matches if the above fails
public IActionResult GetByCategory(string category) { ... }
```

### Solution 2: Use Route Constraints

```csharp
// Restrict {category} to only accept specific predefined values[HttpGet("{category:regex(^(laptop|phone|tablet)$)}")]
public IActionResult GetByCategory(string category) { ... }[HttpGet("top")]  // No conflict anymore
public IActionResult GetTopProducts() { ... }
```

### Solution 3: URL Restructuring

```csharp
// ✅ Best approach — design unambiguous URLs from the start
[HttpGet("top")]                          // /api/products/top
[HttpGet("category/{category}")]          // /api/products/category/laptop
```

---

## Part 7 — Nested Routes

### When to use Nested Routes?

Use them to represent **Parent-Child ownership**. Ask yourself: *"Does this resource exist independently, or does it only make sense in the context of its parent?"*

```text
✅ Use Nested:
/api/orders/10/items         → Items only exist inside an Order
/api/users/5/addresses       → Address belongs to a specific User
/api/blogs/3/comments        → Comments belong to a Blog Post

✅ Do NOT Use Nested:
/api/products                → Products exist independently
/api/categories              → Categories exist independently
```

### Implementing a Nested Route

```csharp
// OrderItemsController.cs
[ApiController][Route("api/orders/{orderId:int}/items")]    // ← Nested route
public class OrderItemsController : ControllerBase
{
    // GET /api/orders/10/items
    [HttpGet]
    public IActionResult GetAll([FromRoute] int orderId)
    {
        var items = _service.GetItemsByOrderId(orderId);
        return Ok(items);
    }

    // GET /api/orders/10/items/3[HttpGet("{itemId:int}")]
    public IActionResult GetById(
        [FromRoute] int orderId,
        [FromRoute] int itemId)
    {
        var item = _service.GetItem(orderId, itemId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    // POST /api/orders/10/items
    [HttpPost]
    public IActionResult Create(
        [FromRoute] int orderId,
        [FromBody] CreateOrderItemDto dto)
    {
        var newItem = _service.CreateItem(orderId, dto);
        return CreatedAtAction(
            nameof(GetById),
            new { orderId, itemId = newItem.Id },
            newItem);
    }

    // DELETE /api/orders/10/items/3
    [HttpDelete("{itemId:int}")]
    public IActionResult Delete(
        [FromRoute] int orderId,
        [FromRoute] int itemId)
    {
        var success = _service.DeleteItem(orderId, itemId);
        if (!success) return NotFound();
        return NoContent();  // 204
    }
}
```

### The Golden Rule of Nested Routes

> **Never nest more than 2 levels deep!** Excessively long URLs are hard to use, document, and test.

```text
✅ Good:        /api/orders/10/items/3
⚠️  Acceptable:  /api/stores/1/products/5/reviews
❌ Too deep:    /api/companies/1/departments/2/teams/3/members/4/tasks/5
```

If you need to access deeply nested resources, create an independent top-level endpoint using query string filters:

```text
// Instead of: GET /api/companies/1/departments/2/teams/3/members
// Use:        GET /api/members?companyId=1&departmentId=2&teamId=3
```

---

## Part 8 — API Versioning via Routes

Versioning is critical once your API has real users to ensure you don't break their apps with new updates.

### Approach 1: Versioning in the URL Path (Most Common)

```csharp
// V1 Controller
[ApiController]
[Route("api/v{version:apiVersion}/products")][ApiVersion("1.0")]
public class ProductsV1Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        // V1 returns old format
        return Ok(new { data = _service.GetAllV1() });
    }
}

// V2 Controller — Introduces new features without breaking V1
[ApiController]
[Route("api/v{version:apiVersion}/products")][ApiVersion("2.0")]
public class ProductsV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        // V2 supports pagination
        return Ok(new {
            data = _service.GetAllV2(page, size),
            pagination = new { page, size, total = _service.Count() }
        });
    }
}
```

### Approach 2: Manual URL Versioning (No extra packages needed)

```csharp
// Simple, manual approach
[ApiController][Route("api/v1/products")]
public class ProductsV1Controller : ControllerBase { ... }

[ApiController][Route("api/v2/products")]
public class ProductsV2Controller : ControllerBase { ... }
```

### Approach 3: Versioning via Query String

```csharp
// GET /api/products?api-version=2.0
[ApiController]
[Route("api/products")]
[ApiVersion("1.0")][ApiVersion("2.0")]
public class ProductsController : ControllerBase
{
    [HttpGet][MapToApiVersion("1.0")]
    public IActionResult GetV1() { ... }[HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2() { ... }
}
```

### Versioning Rules of Thumb

```text
✅ YOU NEED a new version when:
- Changing response payload structures (renaming or removing fields).
- Modifying critical business logic workflows.
- Changing Auth mechanisms.

✅ YOU DO NOT need a new version when:
- Adding new fields to a response (it's backward compatible).
- Adding completely new endpoints.
- Fixing internal bugs that don't affect the interface payload.
```

---

## Part 9 — RESTful API Design (Full CRUD Example)

Bringing all the knowledge together into a production-grade Controller:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/products?category=laptop&sort=price&order=asc&page=1&size=10
    // ─────────────────────────────────────────────────────────────────
    [HttpGet][ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? sort,
        [FromQuery] string? order = "asc",
        [FromQuery] int page = 1,[FromQuery] int size = 10)
    {
        var result = await _service.GetAllAsync(category, sort, order, page, size);
        return Ok(result);
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/products/5
    // ─────────────────────────────────────────────────────────────────
    [HttpGet("{id:int}", Name = "GetProductById")][ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)][ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with id {id} not found" });
        return Ok(product);
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/products
    // ─────────────────────────────────────────────────────────────────
    [HttpPost][ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newProduct = await _service.CreateAsync(dto);

        // Returns 201 Created + auto-generates the 'Location' header
        return CreatedAtRoute(
            routeName: "GetProductById",
            routeValues: new { id = newProduct.Id },
            value: newProduct);
    }

    // ─────────────────────────────────────────────────────────────────
    // PUT /api/products/5  (Full Update)
    // ─────────────────────────────────────────────────────────────────
    [HttpPut("{id:int}")][ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID in URL must match ID in body" });

        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();

        return NoContent();  // 204 — update successful, no body returned
    }

    // ─────────────────────────────────────────────────────────────────
    // PATCH /api/products/5  (Partial Update)
    // ─────────────────────────────────────────────────────────────────
    [HttpPatch("{id:int}")][ProducesResponseType(StatusCodes.Status204NoContent)][ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PartialUpdate(
        [FromRoute] int id,
        [FromBody] JsonPatchDocument<UpdateProductDto> patchDoc)
    {
        var product = await _service.GetForPatchAsync(id);
        if (product == null) return NotFound();

        patchDoc.ApplyTo(product, ModelState);
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _service.SavePatchAsync(id, product);
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────
    // DELETE /api/products/5
    // ─────────────────────────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();

        return NoContent();  // 204 — deletion successful
    }
}
```

### Standard HTTP Status Codes for APIs

| Scenario | Status Code | Meaning |
|---|---|---|
| Read successful | `200 OK` | Returned with data payload |
| Creation successful | `201 Created` | Contains Location header to new item |
| Update/Delete successful | `204 No Content` | Successful, no body returned |
| Invalid data payload | `400 Bad Request` | Validation failed |
| Unauthenticated | `401 Unauthorized` | Missing/invalid token |
| Unauthorized | `403 Forbidden` | Has token but lacks permission |
| Not Found | `404 Not Found` | Resource doesn't exist |
| Data conflict | `409 Conflict` | e.g., Email already exists |
| Server crash | `500 Internal Server Error` | Unhandled exception on server |

---

## Part 10 — Minimal APIs (.NET 6+) Comparison

The core concepts of Routing and Templates (`{}`, `:int`, `?`) remain 100% identical. The only difference is that you **do not use Attributes**; instead, you configure routes via extension methods on the `app` object.

### Controllers vs Minimal APIs (Side-by-side)

```csharp
// ════════════════════════════════════════════════════
// CONTROLLER STYLE
// ════════════════════════════════════════════════════
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? category) { ... }[HttpGet("{id:int}", Name = "GetProductById")]
    public IActionResult GetById([FromRoute] int id) { ... }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        var newProduct = _service.Create(dto);
        return CreatedAtRoute("GetProductById", new { id = newProduct.Id }, newProduct);
    }
}

// ════════════════════════════════════════════════════
// MINIMAL API STYLE (Program.cs)
// ════════════════════════════════════════════════════
var productsApi = app.MapGroup("/api/products");  // Base route grouping

productsApi.MapGet("/", (string? category, IProductService svc) =>
{
    var products = svc.GetAll(category);
    return Results.Ok(products);
});

productsApi.MapGet("/{id:int}", (int id, IProductService svc) =>
{
    var product = svc.GetById(id);
    return product != null ? Results.Ok(product) : Results.NotFound();
}).WithName("GetProductById");                    // Name the route

productsApi.MapPost("/", (CreateProductDto dto, IProductService svc) =>
{
    var newProduct = svc.Create(dto);
    return Results.CreatedAtRoute(
        "GetProductById",
        new { id = newProduct.Id },
        newProduct);
});

productsApi.MapPut("/{id:int}", (int id, UpdateProductDto dto, IProductService svc) =>
{
    var success = svc.Update(id, dto);
    return success ? Results.NoContent() : Results.NotFound();
});

productsApi.MapDelete("/{id:int}", (int id, IProductService svc) =>
{
    var success = svc.Delete(id);
    return success ? Results.NoContent() : Results.NotFound();
});
```

### When to use Minimal APIs vs Controllers?

| Criteria | Minimal API | Controller |
|---|---|---|
| Microservices, small apps | ✅ Superior | A bit verbose |
| Large, multi-layered enterprise apps | Hard to organize | ✅ Superior |
| Need complex Filters/Middleware | Limited | ✅ Full support |
| Complex API versioning | Harder | ✅ Easier |
| Learning, fast prototyping | ✅ Faster setup | Needs boilerplate |

---

## Part 11 — Anti-patterns & Common Mistakes

### ❌ Anti-pattern 1: Using Verbs in URLs

```csharp
// ❌ Bad — URL contains verbs
GET /api/getProducts
GET /api/fetchUserById/5
POST /api/createProduct
DELETE /api/deleteProduct/5

// ✅ Good — URL is a noun, HTTP method acts as the verb
GET /api/products
GET /api/users/5
POST /api/products
DELETE /api/products/5
```

### ❌ Anti-pattern 2: Ignoring Route Constraints

```csharp
// ❌ Dangerous — No ID validation at the route level[HttpGet("{id}")]
public IActionResult GetById(string id)  // Can accept literally any string

// ✅ Safe — Validate immediately at the routing engine
[HttpGet("{id:int:min(1)}")]
public IActionResult GetById(int id)     // Only accepts integer >= 1
```

### ❌ Anti-pattern 3: Relying on Implicit Binding

```csharp
// ❌ ASP.NET tries to guess — Unclear, prone to security bugs[HttpPut("{id}")]
public IActionResult Update(int id, UpdateDto dto) { ... }

// ✅ Explicitly declare data sources[HttpPut("{id:int}")]
public IActionResult Update([FromRoute] int id, [FromBody] UpdateDto dto) { ... }
```

### ❌ Anti-pattern 4: Returning Wrong Status Codes

```csharp
// ❌ Bad — Creation succeeded but returned 200 OK
[HttpPost]
public IActionResult Create([FromBody] CreateDto dto)
{
    var item = _service.Create(dto);
    return Ok(item);  // Should be 201 Created
}

// ✅ Good
[HttpPost]
public IActionResult Create([FromBody] CreateDto dto)
{
    var item = _service.Create(dto);
    return CreatedAtRoute("GetById", new { id = item.Id }, item);  // 201 + Location header
}
```

### ❌ Anti-pattern 5: Over-nesting Routes

```csharp
// ❌ Way too deep — Hard to use and test[Route("api/companies/{companyId}/departments/{deptId}/teams/{teamId}/members")]

// ✅ Better — Create a top-level endpoint relying on query parameters
[Route("api/members")]
// GET /api/members?companyId=1&departmentId=2&teamId=3
```

### ❌ Anti-pattern 6: Hardcoding instead of using Tokens

```csharp
// ❌ Error-prone if you rename the class
[Route("api/products")]
public class ProductItemsController : ControllerBase { }
// If you rename the class, you must manually fix the string

// ✅ Dynamically reflects the class name
[Route("api/[controller]")]
public class ProductItemsController : ControllerBase { }
// → Auto-resolves to: /api/productitems
```

---

## Summary Diagram & Checklist

```text
HTTP Request: GET /api/products/5?sort=price&page=2
                         │
                         ▼
            ┌────────────────────────┐
            │   Routing Middleware   │
            │                        │
            │  ① Matching Phase      │
            │  Base:   "api/[ctrl]"  │
            │  → api/products        │
            │                        │
            │  Template: "{id:int}"  │
            │  → id = 5 (extracted)  │
            │                        │
            │  ② Constraint Check    │
            │  :int → ✅ 5 is an int │
            └────────────────────────┘
                         │
                         ▼
            ┌────────────────────────┐
            │     Data Binding       │
            │                        │
            │  [FromRoute] id = 5    │
            │  [FromQuery] sort = "price"  │
            │  [FromQuery] page = 2  │
            └────────────────────────┘
                         │
                         ▼
            ┌────────────────────────┐
            │  Action Method Exec    │
            │                        │
            │  GetById(id: 5)        │
            │  + inject query params │
            └────────────────────────┘
                         │
                         ▼
            HTTP Response: 200 OK / 404 Not Found
```

### The API Route Design Checklist

```text
□ URLs use Nouns (Resources), not Verbs
□ Exclusively use Attribute Routing (No Conventional)
□ Apply Route Constraints for all numeric parameters
□ Explicitly declare [FromRoute], [FromQuery], [FromBody]
□ Set a Name="" on endpoints for CreatedAtRoute responses
□ Never nest routes deeper than 2 levels
□ Ensure accurate HTTP Status Code returns
□ Implement Versioning if the API anticipates breaking changes
□ Use the [controller] token instead of hardcoding names
□ Resolve AmbiguousMatchExceptions via Order or Constraints
```

---