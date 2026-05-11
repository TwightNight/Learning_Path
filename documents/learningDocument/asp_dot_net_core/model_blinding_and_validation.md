# Validation & Model Binding in ASP.NET Core

---

## Part 1 — What is Model Binding?

When a client sends an HTTP request, the data can come from **multiple places** in the request:

```http
GET /api/products/5?page=2
     ↑ URL path      ↑ query string

POST /api/products
Headers: Content-Type: application/json
Body: { "name": "Laptop", "price": 999 }
         ↑ request body
```

**Model Binding** is the process where ASP.NET Core automatically reads data from those locations and maps it to action method parameters — instead of you manually parsing the request.

---

## Part 2 — Data Sources and Binding Attributes

ASP.NET Core provides 5 attributes to explicitly specify where data should come from.

### 2.1 `[FromRoute]` — Read from URL Path

```http
GET /api/products/5
```

```csharp
[HttpGet("{id}")]
public IActionResult GetById([FromRoute] int id)
// id = 5, taken from {id} in the route template
```

---

### 2.2 `[FromQuery]` — Read from Query String

```http
GET /api/products?page=2&pageSize=10&category=laptop
```

```csharp
[HttpGet]
public IActionResult GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? category = null)
```

You can also bind directly to an object:

```csharp
public class ProductFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Category { get; set; }
    public string? SortBy { get; set; }
}

[HttpGet]
public IActionResult GetAll([FromQuery] ProductFilter filter)
// Automatically maps:
// ?page=2&category=laptop
// → filter.Page = 2
// → filter.Category = "laptop"
```

---

### 2.3 `[FromBody]` — Read from Request Body

Used for POST, PUT, PATCH requests when sending JSON:

```http
POST /api/products
Content-Type: application/json

{ "name": "Laptop", "price": 999, "stock": 50 }
```

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateProductDto dto)
// dto.Name = "Laptop"
// dto.Price = 999
// dto.Stock = 50
```

**Important note:** Each action can only have **one** `[FromBody]` parameter because the request body can only be read once.

---

### 2.4 `[FromHeader]` — Read from HTTP Headers

```http
GET /api/products
X-Request-Id: abc-123
Accept-Language: en-US
```

```csharp
[HttpGet]
public IActionResult GetAll(
    [FromHeader(Name = "X-Request-Id")] string? requestId,
    [FromHeader(Name = "Accept-Language")] string? language)
```

---

### 2.5 `[FromForm]` — Read from Form Data

Used when uploading files or submitting HTML forms:

```csharp
[HttpPost("upload")]
public IActionResult Upload(
    [FromForm] string title,
    [FromForm] IFormFile file)
```

---

## Part 3 — Binding Priority Without Attributes

If you do not specify attributes, ASP.NET Core automatically infers the data source using these rules:

```text
Simple types (int, string, bool...)
    → search Route first → then Query String

Complex types (class, object)
    → search Request Body
```

Example:

```csharp
// ASP.NET Core automatically understands:
// id → from route
// dto → from body
[HttpPut("{id}")]
public IActionResult Update(int id, UpdateProductDto dto)
```

However, it is recommended to explicitly use attributes for clarity and to avoid ambiguity.

---

## Part 4 — DTO (Data Transfer Object)

Before discussing Validation, it is important to understand DTOs because validation is usually applied to DTO classes.

A **DTO (Data Transfer Object)** is a class that only contains the data required for a specific operation — not the full database entity/model.

### Why not use Entities directly?

```csharp
// Database entity — contains sensitive/internal fields
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }   // client should not set this
    public string InternalCode { get; set; }  // internal information
    public bool IsDeleted { get; set; }       // system field
}
```

```csharp
// DTO for POST — only expose allowed fields
public class CreateProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// DTO for PUT — may differ depending on requirements
public class UpdateProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// DTO for response — controls returned data
public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

---

## Part 5 — Validation

Validation is the process of checking whether client input is valid before processing it. If the data is invalid, the API should return `400 Bad Request` with clear error messages.

---

### 5.1 Data Annotations — Built-in Validation

ASP.NET Core provides built-in validation attributes that can be placed on DTO properties:

```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Name must be between 3 and 100 characters")]
    public string Name { get; set; }

    [Required]
    [Range(0.01, 100_000_000,
        ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, 10_000,
        ErrorMessage = "Stock must be between 0 and 10,000")]
    public int Stock { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? ContactEmail { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    public string? ImageUrl { get; set; }

    [RegularExpression(@"^[A-Z]{3}-\d{4}$",
        ErrorMessage = "Code must follow format ABC-1234")]
    public string? ProductCode { get; set; }
}
```

### Common Validation Attributes

| Attribute                           | Validation                 |
| ----------------------------------- | -------------------------- |
| `[Required]`                        | Cannot be null/empty       |
| `[StringLength(max, Min)]`          | String length              |
| `[MinLength(n)]` / `[MaxLength(n)]` | Minimum/maximum length     |
| `[Range(min, max)]`                 | Value range                |
| `[EmailAddress]`                    | Email format               |
| `[Url]`                             | URL format                 |
| `[RegularExpression]`               | Regex pattern              |
| `[Compare("Field")]`                | Compare with another field |
| `[Phone]`                           | Phone number format        |

---

### 5.2 ModelState — Validation Result

After Model Binding completes, ASP.NET Core automatically runs validation and stores the results in `ModelState`:

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateProductDto dto)
{
    // Without [ApiController], manual checking is required
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // With [ApiController], this is handled automatically

    // ... business logic
}
```

The `[ApiController]` attribute automatically returns `400 Bad Request` when validation fails — no manual `if` statement needed.

Example automatic validation response:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Product name is required"],
    "Price": ["Price must be greater than 0"]
  }
}
```

---

### 5.3 Custom Validation Attribute

When validation logic becomes more complex, you can create custom attributes:

```csharp
public class NotInThePastAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext context)
    {
        if (value is DateTime date && date < DateTime.Today)
        {
            return new ValidationResult("Date cannot be in the past");
        }

        return ValidationResult.Success;
    }
}

// Usage
public class CreateEventDto
{
    [Required]
    public string Title { get; set; }

    [NotInThePast]
    public DateTime EventDate { get; set; }
}
```

---

### 5.4 `IValidatableObject` — Multi-field Validation

Used when validation depends on the relationship between multiple fields:

```csharp
public class CreateDiscountDto : IValidatableObject
{
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext context)
    {
        if (DiscountPrice >= OriginalPrice)
        {
            yield return new ValidationResult(
                "Discount price must be lower than original price",
                new[] { nameof(DiscountPrice) });
        }

        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "End date must be after start date",
                new[] { nameof(EndDate) });
        }
    }
}
```

---

### 5.5 FluentValidation — Professional Validation Library

For larger projects, **FluentValidation** is often preferred because it completely separates validation logic from DTOs, making the code cleaner and easier to test.

Install package:

```bash
dotnet add package FluentValidation.AspNetCore
```

Validator example:

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Length(3, 100)
            .WithMessage("Name must be between 3 and 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(100_000_000);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountPrice)
            .LessThan(x => x.Price)
            .WithMessage("Discount price must be lower than original price")
            .When(x => x.DiscountPrice.HasValue);
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
```

---

## Part 6 — Full Request Processing Flow

```text
HTTP Request arrives
      ↓
Model Binding
  ├── [FromRoute]   → data from URL path
  ├── [FromQuery]   → data from query string
  ├── [FromBody]    → data from JSON body
  ├── [FromHeader]  → data from headers
  └── [FromForm]    → data from form/file
      ↓
Validation runs automatically
  ├── Data Annotations ([Required], [Range]...)
  ├── IValidatableObject (multi-field rules)
  └── FluentValidation (if configured)
      ↓
ModelState.IsValid?
  ├── false → 400 Bad Request
  └── true  → execute Action Method
      ↓
Action Method
  └── return Response
```

---

## Summary

| Problem                               | Solution             |
| ------------------------------------- | -------------------- |
| Read data from URL path               | `[FromRoute]`        |
| Read data from query string           | `[FromQuery]`        |
| Read data from JSON body              | `[FromBody]`         |
| Control which fields clients can send | Use DTOs             |
| Simple validation                     | Data Annotations     |
| Validation involving multiple fields  | `IValidatableObject` |
| Complex validation in large projects  | FluentValidation     |
