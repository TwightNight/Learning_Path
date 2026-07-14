# Types in C# — Classes

## 1. What is a Class?

A **class** is a user-defined **reference type** that serves as a blueprint for creating objects. It combines **data** (fields, properties) and **behavior** (methods) into a single unit.

```csharp
// Blueprint
public class Car { }

// Object — an instance created from the blueprint
Car myCar = new Car();
```

### Reference Type vs Value Type

| Characteristic      | `class` (Reference Type)     | `struct` (Value Type)            |
|---------------------|------------------------------|----------------------------------|
| Storage             | Heap                         | Stack                            |
| Assignment `=`      | Copies the reference         | Copies the entire value          |
| Default value       | `null`                       | Cannot be `null` (by default)    |
| Inheritance         | Yes                          | No (can only implement interfaces)|

```csharp
Car a = new Car { Brand = "Toyota" };
Car b = a;           // b points to the same object as a
b.Brand = "Honda";

Console.WriteLine(a.Brand); // "Honda" — same reference!
```

---

## 2. Declaring a Class

### Full Syntax

```csharp
[access_modifier] [modifier] class ClassName [: BaseClass] [, Interface1, ...]
{
    // Fields
    // Constructors
    // Properties
    // Methods
    // Events
    // Nested Types
}
```

### Access Modifiers

| Modifier             | Accessible From                                    |
|----------------------|----------------------------------------------------|
| `public`             | Anywhere                                           |
| `internal`           | Same assembly (default if omitted)                 |
| `private`            | Inside the class only (nested classes only)        |
| `protected`          | The class and its derived classes                  |
| `protected internal` | Derived classes **or** same assembly               |
| `private protected`  | Derived classes **and** same assembly              |

---

## 3. Class Members

### 3.1 Fields

```csharp
public class BankAccount
{
    // Instance field
    private decimal _balance;

    // Static field — shared across all instances
    private static int _totalAccounts = 0;

    // Readonly field — can only be assigned in the constructor
    private readonly string _accountId;

    // Constant — compile-time constant
    private const decimal MIN_BALANCE = 0m;

    public BankAccount(string id)
    {
        _accountId = id;           // OK inside constructor
        _totalAccounts++;
    }
}
```

### 3.2 Properties

Properties act as a bridge between fields and the outside world, exposing controlled getters and setters.

```csharp
public class Product
{
    private string  _name;
    private decimal _price;

    // Full property — full control with validation
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");
            _name = value;
        }
    }

    // Auto-implemented property
    public string SKU { get; set; }

    // Init-only property (C# 9+) — settable only during initialization
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Computed / expression-bodied property
    public bool IsExpensive => _price > 1_000;

    // Private setter
    public decimal Price
    {
        get => _price;
        private set => _price = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException();
    }

    public void ApplyDiscount(decimal percent)
        => Price = _price * (1 - percent / 100);
}
```

### 3.3 Constructors

```csharp
public class Person
{
    public string Name { get; }
    public int    Age  { get; }

    // Default constructor — delegates to the parameterized one
    public Person() : this("Unknown", 0) { }

    // Parameterized constructor
    public Person(string name, int age)
    {
        Name = name;
        Age  = age;
    }

    // Copy constructor
    public Person(Person other) : this(other.Name, other.Age) { }

    // Static constructor — runs once before the first instance is created
    static Person()
    {
        Console.WriteLine("Person class initialized.");
    }
}
```

**Object Initializer** — initialize without a multi-parameter constructor:

```csharp
var p = new Person { Name = "Alice", Age = 22 };
```

**Primary Constructor (C# 12):**

```csharp
public class Person(string name, int age)
{
    public string Name { get; } = name;
    public int    Age  { get; } = age;
}
```

### 3.4 Methods

```csharp
public class MathHelper
{
    // Instance method
    public double CircleArea(double radius) => Math.PI * radius * radius;

    // Static method
    public static int Factorial(int n) =>
        n <= 1 ? 1 : n * Factorial(n - 1);

    // Method overloading
    public string Format(int    value) => $"Int: {value}";
    public string Format(double value) => $"Double: {value:F2}";
    public string Format(string value) => $"String: {value}";

    // ref / out / in parameters
    public bool TryDivide(int a, int b, out double result)
    {
        if (b == 0) { result = 0; return false; }
        result = (double)a / b;
        return true;
    }

    // params — variable number of arguments
    public int Sum(params int[] numbers) => numbers.Sum();
}

// Usage
var h = new MathHelper();
if (h.TryDivide(10, 3, out double r))
    Console.WriteLine(r); // 3.3333...

Console.WriteLine(h.Sum(1, 2, 3, 4, 5)); // 15
```

---

## 4. Static Class and Static Members

```csharp
// Static class — cannot be instantiated or inherited
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return string.Join(" ",
            input.Split(' ')
                 .Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));
    }
}

// Extension method usage
Console.WriteLine("hello world".ToTitleCase()); // "Hello World"
```

```csharp
public class Counter
{
    // Static — shared across all instances
    private static int _count = 0;
    public  static int Count => _count;

    // Instance — unique per object
    public int Id { get; }

    public Counter() => Id = ++_count;
}

var c1 = new Counter(); // Count = 1
var c2 = new Counter(); // Count = 2
Console.WriteLine(Counter.Count); // 2
```

---

## 5. Inheritance

```csharp
public class Animal
{
    public string Name { get; set; }

    public Animal(string name) => Name = name;

    // virtual — allows derived classes to override
    public virtual string Speak() => "...";

    public override string ToString() => $"{GetType().Name}({Name})";
}

public class Dog : Animal
{
    public string Breed { get; set; }

    public Dog(string name, string breed) : base(name) => Breed = breed;

    public override string Speak() => "Woof!";
}

public class Cat : Animal
{
    public Cat(string name) : base(name) { }

    public override string Speak() => "Meow!";
}

// Polymorphism
List<Animal> animals = [new Dog("Rex", "Husky"), new Cat("Mimi")];
foreach (var a in animals)
    Console.WriteLine($"{a.Name}: {a.Speak()}");
// Rex: Woof!
// Mimi: Meow!
```

### `new` vs `override`

```csharp
public class Base
{
    public virtual string Method() => "Base.Method (virtual)";
    public         string Other()  => "Base.Other";
}

public class Derived : Base
{
    public override string Method() => "Derived.Method (override)"; // True polymorphism
    public new      string Other()  => "Derived.Other (hides base)"; // Hiding, not overriding
}

Base obj = new Derived();
Console.WriteLine(obj.Method()); // "Derived.Method (override)" — polymorphism works
Console.WriteLine(obj.Other());  // "Base.Other"                — no polymorphism
```

---

## 6. Abstract Class

An abstract class cannot be instantiated directly. It serves as a base class that enforces a contract while sharing common logic.

```csharp
public abstract class Shape
{
    public string Color { get; set; } = "Black";

    // Abstract — must be implemented by derived classes
    public abstract double Area();
    public abstract double Perimeter();

    // Concrete method — shared behavior
    public void Describe()
        => Console.WriteLine($"{GetType().Name}: Area={Area():F2}, Perimeter={Perimeter():F2}");
}

public class Circle : Shape
{
    public double Radius { get; }
    public Circle(double radius) => Radius = radius;

    public override double Area()      => Math.PI * Radius * Radius;
    public override double Perimeter() => 2 * Math.PI * Radius;
}

public class Rectangle : Shape
{
    public double Width  { get; }
    public double Height { get; }
    public Rectangle(double w, double h) { Width = w; Height = h; }

    public override double Area()      => Width * Height;
    public override double Perimeter() => 2 * (Width + Height);
}

// Shape s = new Shape(); // ❌ Compile error
Shape s = new Circle(5);
s.Describe(); // Circle: Area=78.54, Perimeter=31.42
```

---

## 7. Sealed Class

`sealed` prevents a class from being inherited. Commonly used for security, the Singleton pattern, or performance optimizations.

```csharp
public sealed class Singleton
{
    private static readonly Singleton _instance = new();
    public  static          Singleton Instance => _instance;

    private Singleton() { }

    public void DoWork() => Console.WriteLine("Working...");
}

// public class MySingleton : Singleton { } // ❌ Compile error
Singleton.Instance.DoWork();
```

---

## 8. Partial Class

Splits a single class across multiple files. Common in code generation scenarios (EF Core scaffolding, WinForms designer).

```csharp
// File: Order.cs
public partial class Order
{
    public int    Id       { get; set; }
    public string Customer { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}

// File: Order.Logic.cs
public partial class Order
{
    public decimal Total   => Items.Sum(i => i.Subtotal);
    public bool    IsEmpty => Items.Count == 0;

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Items.Add(item);
    }
}
```

---

## 9. Record Class (C# 9+)

A `record` is a special class optimized for **immutable data** with built-in **value-based equality**.

```csharp
// Positional record — auto-generates constructor, Deconstruct, ToString, ==
public record Point(double X, double Y);

// Record with additional logic
public record Money(decimal Amount, string Currency)
{
    // Validation during initialization
    public decimal Amount { get; } = Amount >= 0
        ? Amount
        : throw new ArgumentException("Amount must be non-negative.");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Currency mismatch.");
        return this with { Amount = Amount + other.Amount };
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
}

var a = new Point(1, 2);
var b = new Point(1, 2);
Console.WriteLine(a == b);       // true  (value equality)
Console.WriteLine(a.Equals(b));  // true

var price = new Money(100, "USD");
var total = price.Add(new Money(50, "USD"));
Console.WriteLine(total); // 150 USD

// Non-destructive mutation with "with" expression
var p2 = a with { Y = 10 };
Console.WriteLine(p2); // Point { X = 1, Y = 10 }
```

---

## 10. Nested Class

```csharp
public class LinkedList<T>
{
    // Nested class — Node is an implementation detail; only LinkedList can see it
    private class Node
    {
        public T    Value { get; set; }
        public Node Next  { get; set; }
        public Node(T value) => Value = value;
    }

    private Node _head;
    private int  _count;

    public void AddFirst(T value)
    {
        var node = new Node(value) { Next = _head };
        _head = node;
        _count++;
    }

    public int Count => _count;
}
```

---

## 11. Generic Class

```csharp
public class Repository<T> where T : class
{
    private readonly List<T> _store = [];

    public void              Add(T entity)   => _store.Add(entity);
    public T?                GetById(int i)  => i < _store.Count ? _store[i] : null;
    public IReadOnlyList<T>  GetAll()        => _store.AsReadOnly();
    public void              Remove(T entity)=> _store.Remove(entity);
    public int               Count           => _store.Count;
}

// Usage
var repo = new Repository<Product>();
repo.Add(new Product { Name = "Laptop", SKU = "LT001" });
Console.WriteLine(repo.Count); // 1
```

---

## 12. Real-World Example — Novel Management System

Integrating all the concepts above into a cohesive domain model.

```csharp
// ── Domain Base ───────────────────────────────────────────────────

public abstract class BaseEntity
{
    public int      Id        { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set;  } = DateTime.UtcNow;
}

// ── Entities ─────────────────────────────────────────────────────

public class Novel : BaseEntity
{
    private readonly List<Chapter> _chapters = [];

    public string Title       { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author      { get; set; } = string.Empty;
    public bool   IsPublished { get; private set; }

    public IReadOnlyList<Chapter> Chapters      => _chapters.AsReadOnly();
    public int                    ChapterCount  => _chapters.Count;
    public double                 AverageRating =>
        _chapters.Where(c => c.Rating > 0)
                 .Select(c => (double)c.Rating)
                 .DefaultIfEmpty(0)
                 .Average();

    public Chapter AddChapter(string title, string content)
    {
        if (!IsPublished)
            throw new InvalidOperationException("Publish the novel before adding chapters.");

        var chapter = new Chapter
        {
            Id      = _chapters.Count + 1,
            Title   = title,
            Content = content,
            Order   = _chapters.Count + 1
        };
        _chapters.Add(chapter);
        UpdatedAt = DateTime.UtcNow;
        return chapter;
    }

    public void Publish()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("A title is required before publishing.");
        IsPublished = true;
        UpdatedAt   = DateTime.UtcNow;
    }
}

public class Chapter : BaseEntity
{
    public string Title    { get; set; } = string.Empty;
    public string Content  { get; set; } = string.Empty;
    public int    Order    { get; set; }
    public int    Rating   { get; set; }       // 1–5
    public int    ViewCount { get; private set; }

    public void IncrementView() => ViewCount++;
}

// ── Value Object (record) ────────────────────────────────────────

public record NovelStats(int TotalChapters, int TotalViews, double AvgRating)
{
    public override string ToString() =>
        $"Chapters: {TotalChapters} | Views: {TotalViews:N0} | Rating: {AvgRating:F1}★";
}

// ── Service (sealed — no need to extend) ─────────────────────────

public sealed class NovelService
{
    private readonly Repository<Novel> _repo = new();

    public Novel Create(string title, string author, string description)
    {
        var novel = new Novel
        {
            Id          = _repo.Count + 1,
            Title       = title,
            Author      = author,
            Description = description
        };
        _repo.Add(novel);
        return novel;
    }

    public NovelStats GetStats(Novel novel) => new(
        TotalChapters : novel.ChapterCount,
        TotalViews    : novel.Chapters.Sum(c => c.ViewCount),
        AvgRating     : novel.AverageRating
    );
}

// ── Program ──────────────────────────────────────────────────────

var service = new NovelService();

var novel = service.Create(
    title       : "Dragon's Mark",
    author      : "John Doe",
    description : "A young man's journey into the world of cultivation...");

novel.Publish();

var ch1 = novel.AddChapter("Chapter 1 — Awakening",  "Content of chapter 1...");
var ch2 = novel.AddChapter("Chapter 2 — The Road",   "Content of chapter 2...");
var ch3 = novel.AddChapter("Chapter 3 — The Trial",  "Content of chapter 3...");

ch1.Rating = 5; ch1.IncrementView(); ch1.IncrementView();
ch2.Rating = 4; ch2.IncrementView();
ch3.Rating = 5; ch3.IncrementView(); ch3.IncrementView(); ch3.IncrementView();

var stats = service.GetStats(novel);

Console.WriteLine($"📖 {novel.Title} by {novel.Author}");
Console.WriteLine($"   {stats}");
// 📖 Dragon's Mark by John Doe
//    Chapters: 3 | Views: 5 | Rating: 4.7★
```

---

## 13. Quick Reference

| Concept           | Keyword(s)               | When to Use                                            |
|-------------------|--------------------------|--------------------------------------------------------|
| Basic class       | `class`                  | Entities, services, helpers                            |
| Inheritance       | `: BaseClass`            | Sharing common logic among related types               |
| Abstract class    | `abstract`               | Base class with shared logic + enforced contract       |
| Sealed class      | `sealed`                 | Prevent inheritance (Singleton, security)              |
| Partial class     | `partial`                | Code-gen, splitting large files                        |
| Record class      | `record`                 | Immutable data, DTOs, Value Objects                    |
| Static class      | `static class`           | Utility methods, Extension methods                     |
| Generic class     | `class Foo<T>`           | Repository, Collection, general-purpose services       |
| Nested class      | class inside class       | Hiding implementation details                          |

---

> 💡 **Practical Rules:**
> - Use **`record`** for DTOs and Value Objects (immutable by design).
> - Use **`abstract`** when subclasses must fulfill a specific contract.
> - Use **`sealed`** for Singletons and classes that should never be extended.
> - Always **encapsulate** fields behind properties with validation logic.
> - Prefer **composition over inheritance** when there is no true IS-A relationship.
> - Keep **static classes** stateless — they should be pure utility containers.