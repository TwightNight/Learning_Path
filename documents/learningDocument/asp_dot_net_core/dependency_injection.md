# Dependency Injection & Service Lifetime in ASP.NET Core

> **Version:** Complete & Extended — covers the problem DI solves, the three lifetimes, dangerous pitfalls, real-world service layer organisation, and advanced registration techniques.

---

## Section 1 — The Problem Before DI

Consider this code:

```csharp
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateOrder(CreateOrderDto dto)
    {
        var dbContext    = new AppDbContext();           // manually created
        var emailService = new EmailService();           // manually created
        var logger       = new FileLogger();             // manually created
        var orderService = new OrderService(dbContext,
                               emailService, logger);   // manually created

        orderService.Create(dto);
        return Ok();
    }
}
```

This pattern has four serious problems:

**① Tight coupling** — `OrdersController` knows exactly how to construct every dependency. To swap `FileLogger` for `CloudLogger`, you must modify the controller itself.

**② Untestable** — You cannot replace the real `EmailService` with a mock during unit tests. Real emails will be sent every time the test runs.

**③ No lifecycle management** — A new `AppDbContext` is created for every request with no mechanism for reuse or cleanup. Database connections are leaked.

**④ Violates Single Responsibility** — The controller is responsible for both handling HTTP requests *and* wiring up its own dependencies. These are two separate concerns.

---

## Section 2 — What is Dependency Injection?

**Dependency Injection (DI)** is a technique in which an object **does not create its own dependencies** — instead, those dependencies are **provided from the outside**.

```csharp
// Without DI — the class creates its own dependency
public class OrderService
{
    private readonly EmailService _email = new EmailService(); // ❌ tightly coupled
}

// With DI — the dependency is received from outside
public class OrderService
{
    private readonly IEmailService _email;

    public OrderService(IEmailService email)  // ✅ injected from outside
    {
        _email = email;
    }
}
```

**IoC Container (Inversion of Control Container)** is the component that automates this process. It maintains a registry of how to create every service, and automatically resolves and injects dependencies into any class that needs them.

ASP.NET Core has a **built-in IoC Container**, accessible through `builder.Services`.

> **The key mental shift:** classes declare *what they need* (via constructor parameters typed as interfaces), and the container figures out *how to provide it*. The class no longer controls the creation of its dependencies — that control is *inverted* to the container.

---

## Section 3 — The Three Steps of Using DI

### Step 1 — Define an Interface

The interface is the **contract** — it describes what a service can do, without revealing how it does it. Consumers depend on the contract, not the implementation.

```csharp
// Describes capability, not implementation
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

public interface IOrderService
{
    Task<Order> CreateAsync(CreateOrderDto dto);
    Task<Order?> GetByIdAsync(int id);
}
```

### Step 2 — Implement the Interface

```csharp
public class EmailService : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        // Real email sending logic (SMTP, SendGrid, etc.)
    }
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<OrderService> _logger;

    // ASP.NET Core reads this constructor and automatically resolves
    // and injects each parameter from the container.
    // If any dependency is not registered, an exception is thrown at startup.
    public OrderService(
        AppDbContext db,
        IEmailService email,
        ILogger<OrderService> logger)
    {
        _db     = db;
        _email  = email;
        _logger = logger;
    }

    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        var order = new Order { /* ... map from dto */ };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _email.SendAsync(dto.CustomerEmail,
            "Order Confirmed", $"Your order ID is: {order.Id}");

        _logger.LogInformation("Order {Id} created successfully", order.Id);
        return order;
    }
}
```

### Step 3 — Register in the Container

```csharp
// Program.cs
// "When anyone needs IEmailService, create and provide an EmailService."
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderService, OrderService>();
```

From this point on, whenever any class declares `IEmailService` in its constructor, the container automatically creates and injects an `EmailService` — with all of *its* dependencies resolved recursively.

---

## Section 4 — Service Lifetime: When is a Service Created?

This is the **most important and most bug-prone** aspect of DI. When registering a service, you must specify its **lifetime** — which determines when a new instance is created and when an existing one is reused.

ASP.NET Core provides three lifetimes:

---

### 4.1 Transient — New instance every time it is requested

```csharp
builder.Services.AddTransient<IMyService, MyService>();
```

```
Request 1:
  Controller → receives instance A
  Service1   → receives instance B  (different from A)
  Service2   → receives instance C  (different from A and B)

Request 2:
  Controller → receives instance D  (brand new)
  Service1   → receives instance E  (brand new)
```

**Use when:** The service is lightweight, stateless, and each caller needs its own independent instance. Examples: validators, mappers, utility/helper classes, ID generators.

**Avoid for:** Heavy services like `DbContext`. Creating a new database connection on every injection is extremely expensive and exhausting the connection pool.

```csharp
// A good fit for Transient: lightweight, stateless, no shared state
public class SlugGenerator
{
    public string Generate(string title) =>
        title.ToLower().Replace(" ", "-");
}

builder.Services.AddTransient<SlugGenerator>();
```

---

### 4.2 Scoped — One instance per HTTP Request

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
```

```
Request 1:
  Controller → receives instance A
  Service1   → receives instance A  (same instance!)
  Service2   → receives instance A  (same instance!)
  → Request ends → instance A is disposed

Request 2:
  Controller → receives instance B  (brand new)
  Service1   → receives instance B  (same instance within Request 2)
  Service2   → receives instance B  (same instance within Request 2)
```

**Use when:** Multiple classes within the same request need to share state or work on the same unit of work. `DbContext` is the classic example — all database operations within a single request share one connection and one transaction, so partial writes are either committed or rolled back together.

**This is the default lifetime for most services** in a Web API.

```csharp
// DbContext is always Scoped — AddDbContext registers it as Scoped automatically.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

---

### 4.3 Singleton — One instance for the entire application lifetime

```csharp
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

```
Application starts → instance A is created

Request 1:  Controller → receives instance A
Request 2:  Controller → receives instance A  (same instance)
Request 3:  Controller → receives instance A  (same instance)
... until the application shuts down
```

**Use when:** The service holds data shared across the entire application, or is expensive to initialise and should only be created once. Examples: in-memory caches, configuration readers, HTTP client factories, connection pools.

**Critical requirement:** Singletons **must be thread-safe** because multiple concurrent requests access the same instance simultaneously.

```csharp
// A good fit for Singleton: shared in-memory cache
public class InMemoryCacheService : ICacheService
{
    // ConcurrentDictionary is used instead of Dictionary because multiple threads
    // (one per concurrent request) will read and write this collection simultaneously.
    // Dictionary is NOT thread-safe and would cause data corruption under concurrency.
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public void Set(string key, object value) => _cache[key] = value;
    public object? Get(string key) => _cache.GetValueOrDefault(key);
}

builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
```

---

## Section 5 — Visual Comparison of the Three Lifetimes

```
                   Request 1              Request 2
                ┌───────────────┐      ┌───────────────┐
Transient       │  A  │  B  │ C │      │  D  │  E  │ F │   every injection = new instance
                └───────────────┘      └───────────────┘

                ┌───────────────┐      ┌───────────────┐
Scoped          │    A    A   A │      │    B    B   B │   every request = 1 shared instance
                └───────────────┘      └───────────────┘

                ┌───────────────┐      ┌───────────────┐
Singleton       │    A    A   A │      │    A    A   A │   entire app = 1 instance forever
                └───────────────┘      └───────────────┘
```

| Lifetime | Created | Disposed | Best for |
|----------|---------|----------|----------|
| `Transient` | Every injection | End of injection scope | Lightweight, stateless helpers |
| `Scoped` | Once per HTTP request | End of request | DbContext, service layer, repositories |
| `Singleton` | Once at startup | App shutdown | Caches, config readers, HTTP clients |

---

## Section 6 — Captive Dependency — The Most Dangerous Mistake

**Captive Dependency** occurs when a service with a **longer lifetime** injects a service with a **shorter lifetime**. The shorter-lived service gets "captured" — it is never refreshed the way it should be, because the container only creates it once (at the time the longer-lived service is first constructed).

```csharp
// ❌ BUG: Singleton captures a Scoped service
public class ReportService          // registered as Singleton
{
    private readonly IOrderService _orders;   // registered as Scoped ← PROBLEM

    public ReportService(IOrderService orders)
    {
        // _orders is created ONCE when ReportService is first created at startup.
        // It is NEVER replaced between requests.
        // The DbContext inside _orders is reused across hundreds of concurrent requests:
        //   → stale data (change tracker caches entities from request 1 and returns
        //      them to request 2 without hitting the database)
        //   → connection leaks (the connection is held indefinitely)
        //   → race conditions (two requests mutating the same DbContext concurrently)
        _orders = orders;
    }
}
```

**The safe injection rules:**

```
Singleton   can only inject   Singleton
Scoped      can inject        Scoped  +  Transient
Transient   can inject        Singleton  +  Scoped  +  Transient
```

The shorter the lifetime, the more freely it can consume dependencies with any lifetime. The longer the lifetime, the more restricted it is.

ASP.NET Core **automatically detects captive dependencies in the Development environment** and throws an `InvalidOperationException` at startup — preventing the app from running with a broken configuration. This detection is disabled in Production for performance reasons, so always test service registrations in Development first.

```csharp
// To confirm scope validation is active in Development:
// (it is ON by default — this is just for illustration)
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = builder.Environment.IsDevelopment();
    options.ValidateOnBuild = builder.Environment.IsDevelopment();
});
```

**Workaround when a Singleton genuinely needs a Scoped service:** inject `IServiceScopeFactory` instead, and manually create a scope each time you need the service:

```csharp
public class ReportService    // Singleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportService(IServiceScopeFactory scopeFactory)
    {
        // IServiceScopeFactory is itself a Singleton — safe to inject.
        _scopeFactory = scopeFactory;
    }

    public async Task GenerateReportAsync()
    {
        // Create a fresh DI scope — equivalent to a new "request scope".
        // The Scoped service resolved here lives only for the duration of this using block.
        using var scope = _scopeFactory.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        var orders = await orderService.GetAllAsync();
        // ... generate the report

        // scope is disposed here — the Scoped service is cleaned up correctly
    }
}
```

---

## Section 7 — Organising the Service Layer

Real-world projects follow a layered structure that maps cleanly onto DI registrations:

```
Controllers/
    OrdersController.cs      ← receives HTTP request, calls service, returns response

Services/
    Interfaces/
        IOrderService.cs     ← contract (what the service does)
        IEmailService.cs
    OrderService.cs          ← business logic (how it does it)
    EmailService.cs

Repositories/
    Interfaces/
        IOrderRepository.cs
    OrderRepository.cs       ← database access only — no business logic here

DTOs/
    CreateOrderDto.cs        ← input shape (what the client sends)
    OrderResponseDto.cs      ← output shape (what the client receives)

Models/
    Order.cs                 ← EF Core entity / database table
```

Each layer only knows the interface of the layer directly below it — never the concrete implementation, and never a layer two levels down:

```csharp
// Controller — only knows IOrderService (not OrderService, not IOrderRepository)
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var order = await _orderService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }
}

// Service — only knows IOrderRepository and IEmailService (not DbContext directly)
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly IEmailService _email;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repo,
        IEmailService email,
        ILogger<OrderService> logger)
    {
        _repo   = repo;
        _email  = email;
        _logger = logger;
    }

    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        var order = new Order { /* ... map from dto */ };
        await _repo.AddAsync(order);

        await _email.SendAsync(
            dto.CustomerEmail,
            "Order Confirmed",
            $"Your order ID is: {order.Id}");

        _logger.LogInformation("Order {Id} created", order.Id);
        return order;
    }

    public async Task<Order?> GetByIdAsync(int id) =>
        await _repo.GetByIdAsync(id);
}

// Repository — only knows DbContext (no business logic, no email, no logging)
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdAsync(int id) =>
        await _db.Orders.FindAsync(id);
}
```

Register everything in `Program.cs`:

```csharp
// Database context — AddDbContext registers it as Scoped automatically
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories — Scoped because they depend on DbContext (also Scoped)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services — Scoped because they depend on repositories (also Scoped)
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Singleton — shared in-memory cache, thread-safe, no per-request state
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
```

---

## Section 8 — Advanced Registration Techniques

### 8.1 Multiple implementations for the same interface

Register multiple concrete types against the same interface. The container collects all of them, and you inject `IEnumerable<T>` to receive them all. This is the **Strategy** or **Chain of Responsibility** pattern expressed through DI.

```csharp
// Register three notification channels against the same interface
builder.Services.AddScoped<INotificationService, EmailNotification>();
builder.Services.AddScoped<INotificationService, SmsNotification>();
builder.Services.AddScoped<INotificationService, PushNotification>();

// Inject IEnumerable<T> to receive all registered implementations.
// The container resolves them in registration order.
public class NotificationDispatcher
{
    private readonly IEnumerable<INotificationService> _services;

    public NotificationDispatcher(IEnumerable<INotificationService> services)
    {
        _services = services;
    }

    // Fans out to all channels — adding a new channel only requires
    // a new registration in Program.cs, no changes to this class.
    public async Task SendAllAsync(string message)
    {
        foreach (var service in _services)
            await service.SendAsync(message);
    }
}
```

### 8.2 Factory function registration

Use when the concrete type to create depends on runtime configuration, or when the constructor requires arguments that are not themselves registered services.

```csharp
// The factory delegate receives the IServiceProvider, letting you resolve
// other registered services to use as inputs to the constructor.
builder.Services.AddScoped<IPaymentService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var mode = config["Payment:Mode"];

    // Return different implementations based on configuration.
    // This logic runs once per request (Scoped), so environment changes
    // at runtime (e.g. toggling a feature flag) are picked up automatically.
    return mode == "sandbox"
        ? new SandboxPaymentService()
        : new ProductionPaymentService(config["Payment:ApiKey"]!);
});
```

### 8.3 `GetRequiredService` vs `GetService`

These are used when you need to manually resolve a service from the container (e.g. in a factory, a middleware, or application startup code):

```csharp
// GetRequiredService<T> — throws InvalidOperationException if the service is not registered.
// Prefer this in application code: a missing registration is always a programming error
// and should fail loudly rather than silently returning null.
var service = provider.GetRequiredService<IOrderService>();

// GetService<T> — returns null if the service is not registered.
// Use only when the dependency is genuinely optional and you have fallback logic.
var service = provider.GetService<IOrderService>();
if (service is null)
{
    // handle the absence gracefully
}
```

### 8.4 Registering open generic types

Useful for generic repositories or handlers where you don't want to register every closed type individually:

```csharp
// Register the open generic once — works for Repository<Order>,
// Repository<Product>, Repository<Customer>, etc.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Usage — the container closes the generic at resolution time
public class OrderService
{
    public OrderService(IRepository<Order> orderRepo) { ... }
}
```

---

## Section 9 — Full DI Lifecycle Overview

```
Registration (Program.cs)
  builder.Services.Add[Lifetime]<IInterface, ConcreteClass>()
        │
        ▼
IoC Container stores the "recipe":
  "When IOrderService is needed → create OrderService,
   resolve IOrderRepository and IEmailService from the container
   and pass them into the constructor."
        │
        ▼
HTTP Request arrives → Controller needs to be created
  Container reads the controller's constructor
  → resolves each parameter recursively
  → creates/reuses instances according to their registered lifetimes
  → injects them into the constructor
        │
        ▼
Action method runs
        │
        ▼
HTTP Request ends
  → Scoped services:    Dispose() is called — DbContext closes its connection
  → Transient services: Dispose() is called — if they implement IDisposable
  → Singleton services: remain alive for the next request
```

---

## Summary

| Lifetime | New instance created | Disposed | Typical use cases |
|----------|---------------------|----------|-------------------|
| `Transient` | Every injection | End of injection scope | Validators, mappers, lightweight helpers |
| `Scoped` | Once per HTTP request | End of request | `DbContext`, service layer, repositories |
| `Singleton` | Once at app startup | App shutdown | Caches, config readers, HTTP client factories |

**Captive Dependency** — the most dangerous mistake: a Singleton **must not** directly inject a Scoped or Transient service. Use `IServiceScopeFactory` if a Singleton genuinely needs a Scoped service.

**Always depend on interfaces, not concrete classes** — this keeps code testable (swap real implementation for a mock in tests), maintainable (swap implementation without touching consumers), and loosely coupled.

**Lifetime selection checklist:**
- Does the service hold request-specific state (e.g. the current user, a unit of work)? → **Scoped**
- Is the service completely stateless and cheap to create? → **Transient**
- Does the service hold global shared state (cache, config) and must it be thread-safe? → **Singleton**

---
