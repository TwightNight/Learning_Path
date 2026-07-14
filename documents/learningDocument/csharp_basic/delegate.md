# Delegates & Anonymous Methods in C#

---

## Part 1 — What is a Delegate?

### 1.1 The Problem to Solve

In programming, you sometimes want to **pass a function as a parameter** to another function — just like passing an integer or a string, but instead passing a piece of behavior.

```csharp
// You want to write a flexible sort method
// You don't know the comparison logic upfront — let the caller decide
SortProducts(products, ???);   // pass the "comparison logic" here
```

Plain C solves this with **function pointers**. C# solves it with **delegates** — more type-safe and object-oriented.

### 1.2 A Delegate is a Type Reference to a Method

A **delegate** is a **data type** that describes a method's signature — its parameter types and return type. A delegate variable can **hold a reference to any method** with a matching signature.

```csharp
// Declare a delegate — defines the "shape" of the method
delegate int MathOperation(int a, int b);

// Methods with the same signature: takes 2 ints, returns int
int Add(int a, int b)      => a + b;
int Subtract(int a, int b) => a - b;
int Multiply(int a, int b) => a * b;

// Assign a method to a delegate variable
MathOperation op = Add;
Console.WriteLine(op(3, 4));   // 7

op = Subtract;
Console.WriteLine(op(3, 4));   // -1

op = Multiply;
Console.WriteLine(op(3, 4));   // 12
```

A delegate is like a **contract**: *"I accept any method that takes 2 int parameters and returns an int."*

---

## Part 2 — Declaring and Using Delegates

### 2.1 Declaration Syntax

```csharp
// Syntax: delegate <return_type> <DelegateName>(<parameters>);

delegate void Logger(string message);
delegate bool Validator(string input);
delegate string Formatter(object value, string format);
delegate Task<Product> ProductFetcher(int id, CancellationToken ct);
```

### 2.2 Multicast Delegate — One Delegate Calls Multiple Methods

A delegate in C# can hold **multiple methods** at once — invoke it once and all of them execute in order:

```csharp
delegate void Notify(string message);

void SendEmail(string msg) => Console.WriteLine($"Email: {msg}");
void SendSms(string msg)   => Console.WriteLine($"SMS: {msg}");
void LogToFile(string msg) => Console.WriteLine($"Log: {msg}");

// Chain multiple methods with +=
Notify notify = SendEmail;
notify += SendSms;
notify += LogToFile;

notify("New order");
// Email: New order
// SMS:   New order
// Log:   New order

// Remove a method from the chain
notify -= SendSms;
notify("Order updated");
// Email: Order updated
// Log:   Order updated
```

### 2.3 Delegate as a Method Parameter

The most common use case — passing behavior into a method:

```csharp
// Method accepts delegates as parameters
void ProcessOrders(
    List<Order> orders,
    Func<Order, bool> filter,      // delegate for filtering
    Action<Order> onEachOrder)     // delegate for processing
{
    foreach (var order in orders.Where(filter))
        onEachOrder(order);
}

// Pass methods in
bool IsHighValue(Order o) => o.Total > 1_000_000;
void PrintOrder(Order o)  => Console.WriteLine($"#{o.Id}: {o.Total}");

ProcessOrders(orders, IsHighValue, PrintOrder);
```

---

## Part 3 — Built-in Delegates: Func, Action, Predicate

Instead of declaring a custom delegate every time, C# provides 3 built-in generic delegates that cover almost every use case.

### 3.1 Action — No Return Value (void)

```csharp
// Action<T1, T2, ...> — any number of parameters, returns void
Action greet        = () => Console.WriteLine("Hello!");
Action<string> log  = msg => Console.WriteLine($"[LOG] {msg}");
Action<string, int> repeat = (s, n) =>
{
    for (int i = 0; i < n; i++)
        Console.WriteLine(s);
};

greet();                // Hello!
log("An error occurred");   // [LOG] An error occurred
repeat("Hello", 3);     // Hello Hello Hello
```

### 3.2 Func — Returns a Value

```csharp
// Func<T1, T2, ..., TReturn> — parameters + return type
// TReturn is always the LAST type argument

Func<int>              getRandom  = () => new Random().Next(100);
Func<int, int>         square     = x => x * x;
Func<int, int, int>    add        = (a, b) => a + b;
Func<string, bool>     isEmail    = s => s.Contains("@");
Func<Product, decimal> getPrice   = p => p.Price * (1 - p.Discount);

Console.WriteLine(square(5));         // 25
Console.WriteLine(add(3, 4));         // 7
Console.WriteLine(isEmail("a@b.c"));  // True
```

### 3.3 Predicate — Condition Check

```csharp
// Predicate<T> is equivalent to Func<T, bool>
// Purpose-built for filtering and checking

Predicate<int>     isEven   = n => n % 2 == 0;
Predicate<string>  notEmpty = s => !string.IsNullOrWhiteSpace(s);
Predicate<Product> inStock  = p => p.Stock > 0;

Console.WriteLine(isEven(4));     // True
Console.WriteLine(notEmpty(""));  // False

var available = products.FindAll(inStock);  // List.FindAll accepts a Predicate
```

### 3.4 Comparing the Three

```
Action<T>        → takes T, returns nothing  → side effects, logging, sending email
Func<T, TResult> → takes T, returns TResult  → transform, compute
Predicate<T>     → takes T, returns bool     → filter, condition check
```

---

## Part 4 — Anonymous Methods

An **anonymous method** is a method **without a name**, defined inline at the point of use — no need to declare a separate named method elsewhere.

### 4.1 The `delegate` Keyword Syntax (C# 2.0 — old style)

```csharp
// Before C# 3.0 — using the delegate keyword
Func<int, int> square = delegate(int x)
{
    return x * x;
};

Action<string> log = delegate(string msg)
{
    Console.WriteLine($"[{DateTime.Now}] {msg}");
};

// Called normally
Console.WriteLine(square(5));   // 25
log("Server started");
```

### 4.2 Lambda Expressions (C# 3.0 — modern, widely used)

A lambda is a more concise syntax for anonymous methods, using the `=>` operator (read as "goes to" or "arrow"):

```csharp
// Expression lambda — single expression, no {} or return needed
Func<int, int>      square  = x => x * x;
Func<int, bool>     isEven  = x => x % 2 == 0;
Func<int, int, int> add     = (a, b) => a + b;

// Statement lambda — multiple lines, requires {} and return
Func<int, int> factorial = n =>
{
    int result = 1;
    for (int i = 2; i <= n; i++)
        result *= i;
    return result;
};

Console.WriteLine(factorial(5));   // 120
```

### 4.3 Comparing the Three Styles

```csharp
// Same logic — 3 different ways to write it

// Style 1: Named method (traditional)
bool IsAdult(int age) => age >= 18;
var adults = people.Where(IsAdult);

// Style 2: Anonymous method (C# 2.0)
var adults = people.Where(delegate(Person p) { return p.Age >= 18; });

// Style 3: Lambda (C# 3.0+) — shortest, most common
var adults = people.Where(p => p.Age >= 18);
```

---

## Part 5 — Closures: Lambdas That "Remember" Outer Variables

A **closure** is a lambda's ability to **capture** (remember and access) variables from an outer scope — even after that scope has ended.

### 5.1 Basic Closure

```csharp
int multiplier = 3;

Func<int, int> triple = x => x * multiplier;  // captures "multiplier"

Console.WriteLine(triple(5));   // 15

// Changing the outer variable → the lambda sees the new value
multiplier = 10;
Console.WriteLine(triple(5));   // 50  ← lambda uses the current value
```

The lambda does not copy the value — it holds a **reference** to the original variable.

### 5.2 Real-World Use: Factory Functions

```csharp
// Create flexible validators using closures
Func<int, int, Predicate<string>> createLengthValidator =
    (min, max) => s => s.Length >= min && s.Length <= max;

var validateUsername = createLengthValidator(3, 20);
var validatePassword = createLengthValidator(8, 100);

Console.WriteLine(validateUsername("ab"));        // False (too short)
Console.WriteLine(validateUsername("alice"));     // True
Console.WriteLine(validatePassword("12345"));     // False
Console.WriteLine(validatePassword("secure123")); // True
```

```csharp
// Create loggers with a fixed prefix
Func<string, Action<string>> createLogger =
    prefix => msg => Console.WriteLine($"[{prefix}] {msg}");

var apiLogger  = createLogger("API");
var dbLogger   = createLogger("DB");
var authLogger = createLogger("AUTH");

apiLogger("Request received");    // [API] Request received
dbLogger("Query executed");       // [DB] Query executed
authLogger("Token validated");    // [AUTH] Token validated
```

### 5.3 The Closure Loop Trap

```csharp
// ❌ Common bug — capturing the loop variable
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
{
    actions.Add(() => Console.WriteLine(i));
}

// All print 3 because they capture the variable i, not its value at the time
actions.ForEach(a => a());
// 3
// 3
// 3

// ✅ Fix — copy into a local variable in each iteration
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
{
    int captured = i;   // new variable per iteration
    actions.Add(() => Console.WriteLine(captured));
}

actions.ForEach(a => a());
// 0
// 1
// 2
```

---

## Part 6 — Delegates and Lambdas in ASP.NET Core

### 6.1 LINQ — Lambdas Everywhere

```csharp
var products = await _db.Products
    .Where(p => !p.IsDeleted && p.Stock > 0)   // Func<Product, bool>
    .Where(p => category == null
             || p.CategoryCode == category)
    .OrderByDescending(p => p.CreatedAt)        // Func<Product, DateTime>
    .Select(p => new ProductDto                 // Func<Product, ProductDto>
    {
        Id      = p.Id,
        Name    = p.Name,
        Price   = p.Price,
        InStock = p.Stock > 0
    })
    .Take(pageSize)
    .ToListAsync(ct);
```

### 6.2 Middleware — Action and Func

```csharp
// app.Use takes Func<HttpContext, RequestDelegate, Task>
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Request-Id"] =
        Guid.NewGuid().ToString("N")[..8];
    await next(context);
});

// app.Map takes Action<IApplicationBuilder>
app.Map("/health", healthApp =>
{
    healthApp.Run(async ctx =>
        await ctx.Response.WriteAsync("OK"));
});
```

### 6.3 Dependency Injection — Factory Delegate

```csharp
// Register a service with a factory function
builder.Services.AddScoped<IPaymentService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var mode   = config["Payment:Mode"];

    return mode == "sandbox"
        ? new SandboxPaymentService()
        : new ProductionPaymentService(config["Payment:ApiKey"]!);
});
```

### 6.4 Strategy Pattern with Delegates

```csharp
public class OrderProcessor
{
    // Dictionary maps order type → processing behavior
    private readonly Dictionary<string, Func<Order, Task<bool>>> _strategies;

    public OrderProcessor(
        IEmailService email,
        IInventoryService inventory,
        IShippingService shipping)
    {
        _strategies = new()
        {
            ["digital"] = async order =>
            {
                await email.SendDownloadLinkAsync(order);
                return true;
            },
            ["physical"] = async order =>
            {
                var reserved = await inventory.ReserveAsync(order);
                if (!reserved) return false;
                await shipping.CreateShipmentAsync(order);
                return true;
            },
            ["subscription"] = async order =>
            {
                await email.SendWelcomeAsync(order);
                return true;
            }
        };
    }

    public async Task<bool> ProcessAsync(Order order)
    {
        if (!_strategies.TryGetValue(order.Type, out var strategy))
            throw new ArgumentException($"Unsupported order type: {order.Type}");

        return await strategy(order);
    }
}
```

### 6.5 Pipeline Pattern with Func

```csharp
// Build a data processing pipeline using a delegate chain
public class DataPipeline<T>
{
    private Func<T, T> _pipeline = x => x;   // identity function

    public DataPipeline<T> AddStep(Func<T, T> step)
    {
        var current = _pipeline;
        _pipeline = x => step(current(x));   // compose functions
        return this;
    }

    public T Execute(T input) => _pipeline(input);
}

// Usage
var pipeline = new DataPipeline<string>()
    .AddStep(s => s.Trim())
    .AddStep(s => s.ToLower())
    .AddStep(s => s.Replace(" ", "-"));

Console.WriteLine(pipeline.Execute("  Hello World  "));
// hello-world
```

---

## Part 7 — Events: A Special Multicast Delegate

An **event** is a wrapper around a multicast delegate — it restricts access, allowing only `+=` and `-=` from outside the class:

```csharp
public class OrderService
{
    // Declare events using EventHandler (the standard .NET delegate)
    public event EventHandler<OrderCreatedEventArgs>? OrderCreated;
    public event EventHandler<OrderEventArgs>? OrderCancelled;

    public async Task<Order> CreateAsync(CreateOrderDto dto)
    {
        var order = new Order { ... };
        await _repo.AddAsync(order);

        // Raise the event — notify all subscribers
        OrderCreated?.Invoke(this, new OrderCreatedEventArgs(order));

        return order;
    }
}

public class OrderCreatedEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderCreatedEventArgs(Order order) => Order = order;
}

// Subscribe to the event
var orderService = new OrderService(...);

// Handler 1: send confirmation email
orderService.OrderCreated += async (sender, e) =>
    await emailService.SendConfirmationAsync(e.Order);

// Handler 2: update inventory
orderService.OrderCreated += async (sender, e) =>
    await inventoryService.ReserveAsync(e.Order);

// Handler 3: log analytics
orderService.OrderCreated += (sender, e) =>
    logger.LogInformation("Order created: {Id}", e.Order.Id);

// When CreateAsync is called → all 3 handlers run
```

---

## Part 8 — Full Overview

```
Delegate
  └── A type reference to a method with a matching signature
      Declaration: delegate ReturnType Name(Params)

Built-in delegates (no need to declare manually):
  ├── Action<T>        → void, used for side effects
  ├── Func<T, TResult> → has return value, used for transform
  └── Predicate<T>     → bool, used for filtering

Anonymous Methods:
  ├── delegate keyword  → old syntax (C# 2.0)
  └── Lambda =>         → modern syntax (C# 3.0+)
       ├── Expression:  x => x * x
       └── Statement:   x => { ...; return ...; }

Closure:
  └── Lambda captures variables from the outer scope
      Holds a reference, not a copy of the value
      Watch out for loop variable capture

Real-world applications:
  ├── LINQ: Where/Select/OrderBy accept Func/Predicate
  ├── Middleware: app.Use/Map accept Action/Func
  ├── DI: factory registration
  ├── Strategy Pattern: Dictionary<string, Func<T>>
  ├── Pipeline Pattern: compose a Func chain
  └── Event: multicast delegate with access control
```