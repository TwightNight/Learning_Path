# 📖 GUIDE TO ENTITY FRAMEWORK CORE (EF CORE)

---

## 1. Overview: Entity Framework & ORM

### 1.1 What is Entity Framework?

**Entity Framework Core (EF Core)** is a modern, open-source **Object-Relational Mapper (ORM)** for .NET. It allows developers to build a clean, portable Data Access Layer (DAL) at a high level of abstraction using C# — without writing raw SQL.

EF Core supports a wide range of database engines:
- **Relational:** SQL Server (On-prem & Azure), SQLite, MySQL, PostgreSQL, Oracle
- **NoSQL:** Azure Cosmos DB

### 1.2 What is an ORM?

There is a fundamental **impedance mismatch** between how OOP languages and relational databases represent data:

| OOP Concept (C#) | Relational DB Concept (SQL) |
| :--- | :--- |
| **Class** (e.g. `class User`) | **Table** (e.g. `Users`) |
| **Object** (e.g. `new User()`) | **Record / Row** |
| **Property** (e.g. `user.Name`) | **Column** (e.g. `Name` column) |
| **Collection** (e.g. `List<User>`) | **Record Set** |
| **Object Reference / Navigation** | **Relationship / Foreign Key** |

An ORM acts as a **translator** between these two worlds. All `SELECT`, `INSERT`, `UPDATE`, and `DELETE` statements are automatically generated and executed by the ORM.

> 💡 **Insight:** Instead of thinking *"How do I JOIN these 3 tables?"*, you think *"How do I get the list of Posts from this Author object?"*

---

## 2. Core Components

### 2.1 DbContext — The Heart of EF

`DbContext` represents a **session with the database**. It is the central object you interact with for all data operations.

- **Design Pattern:** Combines the **Unit of Work** and **Repository** patterns.
- **Query Translation:** Compiles LINQ expressions into database-specific SQL.
- **Change Tracking:** Monitors entity state and flushes changes to the DB via `SaveChanges()`.

#### DbContext Lifetime (Critical for Web Apps)

In ASP.NET Core, DbContext is registered via Dependency Injection. Choosing the right lifetime is crucial:

| Lifetime | Recommended? | Description |
| :--- | :---: | :--- |
| **Scoped** | ✅ Yes (default) | One instance per HTTP Request. Disposed when the response is sent. Safe and efficient. |
| **Transient** | ⚠️ Rarely | New instance on every injection. Expensive — hard to share transactions. |
| **Singleton** | ❌ Never | One instance shared across all users. Causes memory leaks, data bloat, and thread-safety bugs. |

> ⚠️ **WARNING:** `DbContext` is **NOT thread-safe**. Never execute two concurrent async operations on the same DbContext instance — it throws: *"A second operation started on this context before a previous operation completed"*.

**DbContext Pooling:** Use `AddDbContextPool` to recycle DbContext instances instead of creating/disposing them on every request. This can improve throughput by **20–30%**.

### 2.2 DbSet\<T\> — Your Table Proxy

Each `DbSet<T>` in your DbContext represents one database table, where `T` is the entity class (one row = one object). It provides CRUD methods without writing SQL.

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }  // → 'Blogs' table
    public DbSet<Post> Posts { get; set; }  // → 'Posts' table
}

// Instead of: SELECT * FROM Blogs WHERE Url LIKE '%google%'
// You write:
var blogs = db.Blogs.Where(b => b.Url.Contains("google")).ToList();
```

---

## 3. Model Configuration

EF Core offers two ways to configure how your classes map to the database.

### 3.1 Data Annotations

Attributes placed directly on entity properties. Quick and readable, but mixes DB config with domain logic.

```csharp
public class User
{
    [Key]           // Primary key
    public int UserId { get; set; }

    [Required]      // NOT NULL
    [MaxLength(50)] // VARCHAR(50)
    public string Username { get; set; }
}
```

### 3.2 Fluent API

Method chaining inside `OnModelCreating`. More powerful than Data Annotations and keeps entity classes clean (POCO).

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
        .HasKey(u => u.UserId);

    modelBuilder.Entity<User>()
        .Property(u => u.Username)
        .IsRequired()
        .HasMaxLength(50);
}
```

---

## 4. Relationships

EF Core handles **1-to-1**, **1-to-Many**, and **Many-to-Many** relationships via **Navigation Properties** and **Foreign Keys**.

**Example — One Author has Many Posts:**

```csharp
public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; }

    // Navigation: one author → many posts
    public ICollection<Post> Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }

    public int AuthorId { get; set; }       // Foreign Key
    public Author Author { get; set; }      // Navigation: back to author
}
```

---

## 5. Migrations

**Migrations** are like **version control for your database schema**. When you add a property to a class, the DB table doesn't have that column yet. Instead of manually running `ALTER TABLE` in SQL, Migrations detect the diff and generate the update automatically — **without losing existing data**.

**The two-step workflow (Package Manager Console):**

```bash
# Step 1: Snapshot the pending model changes
Add-Migration AddPhoneNumberToUser

# Step 2: Apply the changes to the database
Update-Database
```

---

## 6. LINQ & Data Loading

### 6.1 What is LINQ in EF Core?

**LINQ (Language Integrated Query)** is the primary way to query data in EF Core. It lets you write type-safe queries in C# that EF Core translates into SQL at runtime.

- **Strongly-typed:** Compile-time checks catch typos and type mismatches — impossible with raw SQL strings.
- **Auto-translated:** EF Core converts LINQ into the correct SQL dialect for your database (SQL Server, PostgreSQL, SQLite, etc.).
- **Abstraction:** Focus on business logic, not JOIN syntax.
- **Modern features:** EF Core 10 adds `LeftJoin`, `RightJoin`, and native JSON column querying via LINQ.

### 6.2 Data Loading Strategies

When querying entities that have related data (e.g. Authors with their Posts), EF Core offers three strategies:

#### Eager Loading — Load everything upfront

Use `.Include()` and `.ThenInclude()`. All data is fetched in a **single SQL query (JOIN)**. Best when you know you will always need the related data.

```csharp
// One query with JOIN — fetches authors and their posts together
var authors = db.Authors
                .Include(a => a.Posts)
                .ToList();
```

#### Lazy Loading — Load on demand (automatic)

Related data is **not loaded initially**. EF fires a new SQL query automatically the moment you access the navigation property in code. Requires `virtual` properties and the proxies package.

```csharp
var author = db.Authors.First();    // SQL #1: fetch author
                                    // author.Posts is empty here

// EF automatically fires SQL #2 when you access .Posts
foreach (var post in author.Posts) { ... }
```

> ⚠️ **N+1 Problem:** If you loop over 100 authors and access `.Posts` on each, EF fires **101 separate SQL queries**. Always profile Lazy Loading carefully.

#### Explicit Loading — Load on demand (manual)

You control exactly when related data is loaded. Useful when you want to **filter** related data before loading it.

```csharp
var author = db.Authors.First();

// Manually load only published posts for this author
db.Entry(author)
  .Collection(a => a.Posts)
  .Query()
  .Where(p => p.IsPublished)
  .Load();
```

### 6.3 Deferred Execution

Writing a LINQ query does **NOT** hit the database immediately. EF builds an expression tree in memory. The SQL is only sent to the DB when the query is **"materialized"** (forced to return results).

| Deferred (no DB call) | Immediate (executes SQL) |
| :--- | :--- |
| `.Where()`, `.OrderBy()`, `.Select()`, `.Take()`, `.Skip()` | `.ToList()`, `.ToArray()`, `.First()`, `.Count()`, `.Any()`, `foreach` |

```csharp
// No database call yet — just building the query
IQueryable<User> query = db.Users.Where(u => u.Age > 18);

if (sortByName)
    query = query.OrderBy(u => u.Name); // Still no DB call

// NOW the SQL fires: SELECT * FROM Users WHERE Age > 18 ORDER BY Name
var result = query.ToList();
```

---

### 6.4 IQueryable vs IEnumerable

This is the most important EF Core concept for performance, and the most common interview topic. Understanding the difference is non-negotiable.

#### The core distinction

| | `IQueryable<T>` | `IEnumerable<T>` |
| :--- | :--- | :--- |
| **Where filtering runs** | On the **DATABASE** (SQL `WHERE`) | In **.NET app memory** (C# LINQ) |
| **Data transferred** | Only matching rows | **ALL rows** in the table |
| **SQL generated** | `SELECT … WHERE …` | `SELECT * FROM …` (no filter) |
| **Memory usage** | Low — DB does the heavy lifting | HIGH — entire table loaded into RAM |
| **Performance on large tables** | ✅ Excellent | ❌ Catastrophic |
| **Composable** | Yes — each `.Where()` adds a SQL clause | Yes — but runs in memory after full load |
| **Typical source** | `DbSet<T>`, `db.Set<T>()` | `List<T>`, arrays, already-loaded data |

#### The classic mistake

The moment you assign a `DbSet` to `IEnumerable`, EF loads the **entire table** into RAM before applying any filter:

```csharp
// ❌ WRONG — catastrophic on large tables
IEnumerable<User> users = db.Users;           // Loads ALL users into RAM!
var adults = users.Where(u => u.Age > 18);    // Filtered in C#, not in SQL

// SQL sent to DB:  SELECT * FROM Users   ← no WHERE clause!


// ✅ CORRECT — filtering happens inside the database
IQueryable<User> users = db.Users;            // Just a query definition, no data yet
var adults = users.Where(u => u.Age > 18);    // Appended to the SQL query

// SQL sent to DB:  SELECT * FROM Users WHERE Age > 18
```

#### Composing queries — IQueryable builds one SQL query

Each `.Where()`, `.OrderBy()`, or `.Take()` call on an `IQueryable` adds to the SQL expression tree. Only one query is ultimately sent:

```csharp
IQueryable<Product> query = db.Products;  // Base query

// Each line refines the SQL — nothing runs yet
query = query.Where(p => p.CategoryId == 5);
query = query.Where(p => p.Price < 100);
query = query.OrderBy(p => p.Name);
query = query.Take(20);

// ONE efficient SQL query sent:
// SELECT TOP 20 * FROM Products
// WHERE CategoryId = 5 AND Price < 100
// ORDER BY Name
var results = query.ToList();
```

With `IEnumerable`, each chained operation runs as a separate C# pass over the already-loaded data in RAM.

#### When IEnumerable IS appropriate

Use `IEnumerable` after you have already materialized the data (called `.ToList()`, etc.) or when working with non-EF in-memory collections:

```csharp
// Data is already in memory — IEnumerable is fine here
List<User> users = db.Users.Where(u => u.Age > 18).ToList();

// Further in-memory filtering is acceptable
IEnumerable<User> admins = users.Where(u => u.Role == "Admin");

// Also fine: plain in-memory collections
IEnumerable<int> ids = new[] { 1, 2, 3, 4, 5 };
```

#### The AsEnumerable() escape hatch

Sometimes EF cannot translate a C# function into SQL. Use `.AsEnumerable()` to deliberately switch from server-side to client-side evaluation at a specific point in the chain:

```csharp
var results = db.Users
    .Where(u => u.Age > 18)             // Runs on DB ← SQL WHERE
    .AsEnumerable()                     // ← Switch to in-memory from here
    .Where(u => MyCustomCSharpFunc(u))  // Runs in RAM (not translatable to SQL)
    .ToList();

// SQL sent:  SELECT * FROM Users WHERE Age > 18
// Then C# applies MyCustomCSharpFunc on the filtered result set in memory
```

> ✅ **Rule of thumb:** Keep your query as `IQueryable<T>` for as long as possible. Apply the most restrictive SQL filters first. Only switch to `IEnumerable` when you explicitly need in-memory operations that EF cannot translate.

---

## 7. Change Tracking & No-Tracking

### 7.1 Change Tracking

When you load an entity from the DB, EF stores a **snapshot** of its original values. On `SaveChanges()`, EF diffs the current state against the snapshot and generates a minimal `UPDATE` targeting only the changed columns.

```csharp
var user = db.Users.First(u => u.Id == 1);
user.Name = "New Name";   // EF marks this entity as 'Modified'

// EF generates: UPDATE Users SET Name = 'New Name' WHERE Id = 1
db.SaveChanges();
```

### 7.2 No-Tracking Queries

Tracking costs memory and CPU. For **read-only scenarios** (dashboards, API list endpoints, reports), use `.AsNoTracking()` to skip snapshot creation entirely.

```csharp
// Faster, less RAM — changes to these objects won't be saved by EF
var products = db.Products.AsNoTracking().ToList();
```

---

## 8. Advanced Features

### 8.1 Bulk Operations (EF Core 7+)

Before EF Core 7, updating 1,000 rows required loading them all into RAM and looping. `ExecuteUpdate` and `ExecuteDelete` now push the operation directly to the DB in a **single SQL statement**.

```csharp
// Increase price of all products in category 1 by 10%
db.Products
  .Where(p => p.CategoryId == 1)
  .ExecuteUpdate(s => s.SetProperty(p => p.Price, p => p.Price * 1.1m));

// Delete all inactive users in one SQL statement
db.Users.Where(u => !u.IsActive).ExecuteDelete();
```

### 8.2 Concurrency Control

EF Core supports **Optimistic Concurrency** via a `[Timestamp]` / `RowVersion` column. If two users load the same record and both try to save, the second save detects the version mismatch and throws `DbUpdateConcurrencyException`, allowing you to handle the conflict gracefully (notify the user, merge data, etc.).

### 8.3 Interceptors

Interceptors are lifecycle hooks that let you tap into EF's pipeline. Common use cases:
- **Logging:** Capture and log every SQL query before it executes.
- **Auditing:** Automatically set `UpdatedAt = DateTime.UtcNow` before every `SaveChanges()`.
- **Soft delete enforcement:** Intercept deletes and convert them to updates.

### 8.4 Global Query Filters

Declared once in `OnModelCreating`, these filters are **automatically appended to every query** against that entity. Ideal for **Soft Delete** and **Multi-tenancy**.

```csharp
// In OnModelCreating:
modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);

// Every query against Post now implicitly includes WHERE IsDeleted = 0
// No need to add .Where(p => !p.IsDeleted) everywhere manually
```

### 8.5 Transactions

`SaveChanges()` already wraps all changes in a single transaction by default. For scenarios spanning multiple `SaveChanges()` calls, manage the transaction explicitly:

```csharp
using var transaction = db.Database.BeginTransaction();
try
{
    db.Orders.Add(newOrder);
    db.SaveChanges();

    db.Inventory.Update(stock);
    db.SaveChanges();

    transaction.Commit();   // All steps succeeded
}
catch
{
    transaction.Rollback(); // Any failure → revert everything
}
```

---
