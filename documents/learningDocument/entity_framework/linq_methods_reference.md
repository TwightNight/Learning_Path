# 📖 LINQ Methods — Complete Reference Guide

> All examples use EF Core (`IQueryable<T>`) unless noted. Methods marked with 🔵 execute immediately (materialize the query); all others are deferred.

---

## Table of Contents

1. [Filtering](#1-filtering)
2. [Projection](#2-projection)
3. [Ordering](#3-ordering)
4. [Paging & Partitioning](#4-paging--partitioning)
5. [Aggregation](#5-aggregation) 🔵
6. [Existence & Quantifiers](#6-existence--quantifiers) 🔵
7. [Element Access](#7-element-access) 🔵
8. [Joins](#8-joins)
9. [Grouping](#9-grouping)
10. [Set Operations](#10-set-operations)
11. [Flattening](#11-flattening)
12. [Materialization](#12-materialization) 🔵
13. [Conversion & Casting](#13-conversion--casting)
14. [String & Value Helpers (EF-specific)](#14-string--value-helpers-ef-specific)

---

## 1. Filtering

### `.Where(predicate)`

Filters a sequence to only include elements that match the condition. Translates to a SQL `WHERE` clause.

```csharp
// Get all active users over 18
var users = db.Users
              .Where(u => u.IsActive && u.Age > 18)
              .ToList();
// SQL: SELECT * FROM Users WHERE IsActive = 1 AND Age > 18
```

Multiple `.Where()` calls are AND-ed together:

```csharp
var query = db.Products.Where(p => p.CategoryId == 5);
query = query.Where(p => p.Price < 100);
// SQL: WHERE CategoryId = 5 AND Price < 100
```

### `.OfType<T>()`

Filters elements to only those of a specific type. Most useful on in-memory collections with inheritance.

```csharp
// From a mixed list, get only PremiumUser instances
IEnumerable<User> users = db.Users.ToList();
var premiumOnly = users.OfType<PremiumUser>();
```

---

## 2. Projection

### `.Select(selector)`

Transforms each element into a new shape. Translates to a SQL `SELECT` clause. Use this to avoid loading unnecessary columns.

```csharp
// Select only Id and Name — avoids loading large columns like ProfilePhoto
var names = db.Users
              .Select(u => new { u.Id, u.Name })
              .ToList();
// SQL: SELECT Id, Name FROM Users
```

Project into a dedicated DTO:

```csharp
var dtos = db.Users
             .Select(u => new UserDto
             {
                 FullName = u.FirstName + " " + u.LastName,
                 Email    = u.Email
             })
             .ToList();
```

### `.SelectMany(selector)`

Flattens a collection of collections into a single sequence. Useful for 1-to-many navigation properties.

```csharp
// Get every post from every author in one flat list
var allPosts = db.Authors
                 .SelectMany(a => a.Posts)
                 .ToList();
// SQL: SELECT p.* FROM Posts p INNER JOIN Authors a ON ...
```

---

## 3. Ordering

### `.OrderBy(keySelector)`

Sorts elements in **ascending** order. Translates to `ORDER BY column ASC`.

```csharp
var users = db.Users.OrderBy(u => u.LastName).ToList();
// SQL: SELECT * FROM Users ORDER BY LastName ASC
```

### `.OrderByDescending(keySelector)`

Sorts elements in **descending** order. Translates to `ORDER BY column DESC`.

```csharp
var products = db.Products.OrderByDescending(p => p.Price).ToList();
// SQL: SELECT * FROM Products ORDER BY Price DESC
```

### `.ThenBy(keySelector)`

Applies a **secondary ascending sort** after `OrderBy` / `OrderByDescending`.

```csharp
var users = db.Users
              .OrderBy(u => u.LastName)
              .ThenBy(u => u.FirstName)
              .ToList();
// SQL: ORDER BY LastName ASC, FirstName ASC
```

### `.ThenByDescending(keySelector)`

Applies a **secondary descending sort**.

```csharp
var products = db.Products
                 .OrderBy(p => p.Category)
                 .ThenByDescending(p => p.Price)
                 .ToList();
// SQL: ORDER BY Category ASC, Price DESC
```

### `.Reverse()`

Reverses the order of the sequence. Note: EF Core may not translate this to SQL — it often falls back to in-memory. Prefer `OrderByDescending` for DB-side sorting.

```csharp
// Works reliably on in-memory collections
var reversed = myList.Reverse();
```

---

## 4. Paging & Partitioning

These three methods are the backbone of pagination.

### `.Skip(count)`

Skips the first `n` elements. Translates to SQL `OFFSET`.

```csharp
var page2 = db.Products.OrderBy(p => p.Id).Skip(10).ToList();
// SQL: SELECT * FROM Products ORDER BY Id OFFSET 10 ROWS
```

> ⚠️ **Always pair `.Skip()` with `.OrderBy()`** — without a defined order, SQL Server can return results in any order and your pages will be inconsistent.

### `.Take(count)`

Takes only the first `n` elements. Translates to SQL `TOP` or `FETCH NEXT`.

```csharp
var top5 = db.Products.OrderBy(p => p.Price).Take(5).ToList();
// SQL: SELECT TOP 5 * FROM Products ORDER BY Price
```

### Pagination pattern — Skip + Take together

```csharp
int pageNumber = 2;   // 1-indexed
int pageSize   = 10;

var page = db.Products
             .OrderBy(p => p.Id)
             .Skip((pageNumber - 1) * pageSize)  // Skip(10)
             .Take(pageSize)                      // Take(10)
             .ToList();
// SQL: SELECT * FROM Products ORDER BY Id
//      OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
```

### `.TakeLast(count)`

Takes the last `n` elements. Typically resolved in memory — prefer `OrderByDescending + Take` for DB efficiency.

```csharp
// Prefer this for DB:
var last5 = db.Products.OrderByDescending(p => p.Id).Take(5).ToList();
```

### `.SkipWhile(predicate)` / `.TakeWhile(predicate)`

Skips or takes elements while a condition is true, then stops. **Not translatable to SQL** — runs in memory only.

```csharp
var items = list.TakeWhile(x => x.Price < 100);  // In-memory only
```

---

## 5. Aggregation 

All aggregation methods execute immediately and return a scalar value, not a collection.

### `.Count()` / `.CountAsync()`

Returns the total number of matching records. Translates to `SELECT COUNT(*)`.

```csharp
int total = db.Users.Count();
// SQL: SELECT COUNT(*) FROM Users

int activeCount = db.Users.Count(u => u.IsActive);
// SQL: SELECT COUNT(*) FROM Users WHERE IsActive = 1
```

### `.LongCount()`

Same as `.Count()` but returns `long`. Use when the count could exceed `int.MaxValue` (~2.1 billion).

```csharp
long total = db.Logs.LongCount();
```

### `.Sum(selector)`

Returns the sum of a numeric property. Translates to `SELECT SUM(column)`.

```csharp
decimal totalRevenue = db.Orders.Sum(o => o.Amount);
// SQL: SELECT SUM(Amount) FROM Orders
```

### `.Average(selector)`

Returns the average of a numeric property. Translates to `SELECT AVG(column)`.

```csharp
double avgAge = db.Users.Average(u => u.Age);
// SQL: SELECT AVG(Age) FROM Users
```

### `.Min(selector)` / `.Max(selector)`

Returns the minimum or maximum value. Translates to `SELECT MIN()` / `SELECT MAX()`.

```csharp
decimal cheapest  = db.Products.Min(p => p.Price);
decimal mostExp   = db.Products.Max(p => p.Price);
// SQL: SELECT MIN(Price) FROM Products
// SQL: SELECT MAX(Price) FROM Products
```

### `.Aggregate(seed, func)` *(in-memory only)*

Applies a custom accumulator function over a sequence. Not translatable to SQL.

```csharp
// Multiply all quantities together
var product = new[] { 2, 3, 4 }.Aggregate(1, (acc, x) => acc * x);
// Result: 24
```

---

## 6. Existence & Quantifiers 

### `.Any()` / `.Any(predicate)`

Returns `true` if **any** element matches. Far more efficient than `.Count() > 0` — translates to `SELECT TOP 1` or `EXISTS`.

```csharp
bool hasUsers = db.Users.Any();
// SQL: SELECT CASE WHEN EXISTS (SELECT 1 FROM Users) THEN 1 ELSE 0 END

bool hasAdmins = db.Users.Any(u => u.Role == "Admin");
// SQL: ... WHERE Role = 'Admin'
```

> ✅ **Always prefer `.Any()` over `.Count() > 0`** for existence checks.

### `.All(predicate)`

Returns `true` if **every** element matches the condition.

```csharp
bool allActive = db.Users.All(u => u.IsActive);
// SQL: SELECT CASE WHEN NOT EXISTS (SELECT 1 FROM Users WHERE IsActive = 0)
//           THEN 1 ELSE 0 END
```

### `.Contains(value)` 🔵

Checks whether a specific value exists in the sequence. On `IQueryable` with a primitive collection, translates to `IN (...)`.

```csharp
// Check if a specific user ID exists
bool exists = db.Users.Select(u => u.Id).Contains(42);
// SQL: SELECT CASE WHEN 42 IN (SELECT Id FROM Users) ...

// More common: filter by a list of IDs
var ids    = new[] { 1, 2, 3 };
var users  = db.Users.Where(u => ids.Contains(u.Id)).ToList();
// SQL: SELECT * FROM Users WHERE Id IN (1, 2, 3)
```

---

## 7. Element Access 

All of these execute immediately and return a single element.

### `.First()` / `.FirstOrDefault()`

Returns the **first** element, optionally matching a predicate.

| Method | If no match found |
| :--- | :--- |
| `.First()` | Throws `InvalidOperationException` |
| `.FirstOrDefault()` | Returns `null` (reference types) or `default` |

```csharp
var user  = db.Users.OrderBy(u => u.Name).First();
var admin = db.Users.FirstOrDefault(u => u.Role == "Admin");
// SQL: SELECT TOP 1 * FROM Users WHERE Role = 'Admin'
```

### `.Last()` / `.LastOrDefault()`

Returns the **last** element. Requires `OrderBy` to be meaningful. EF Core translates this by reversing the order.

```csharp
var newest = db.Posts.OrderBy(p => p.CreatedAt).Last();
// SQL: SELECT TOP 1 * FROM Posts ORDER BY CreatedAt DESC
```

### `.Single()` / `.SingleOrDefault()`

Returns exactly **one** element. Throws if zero or more than one element is found.

| Method | Zero matches | More than one match |
| :--- | :--- | :--- |
| `.Single()` | Throws | Throws |
| `.SingleOrDefault()` | Returns `null` / `default` | Throws |

```csharp
// Use when you expect exactly one result (e.g., lookup by unique key)
var user = db.Users.Single(u => u.Email == "john@example.com");
```

> 💡 **When to use which:**
> - `.First()` — you want the first result, there may be many (most common)
> - `.Single()` — you expect exactly one result and want validation
> - `.FirstOrDefault()` — the result might not exist (most common for lookups)
> - `.SingleOrDefault()` — might not exist, but must not have duplicates

### `.ElementAt(index)` / `.ElementAtOrDefault(index)`

Returns the element at a specific index position.

```csharp
var third = db.Products.OrderBy(p => p.Id).ElementAt(2);
// SQL: SELECT * FROM Products ORDER BY Id OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY
```

---

## 8. Joins

### `.Join()` — Inner Join

Combines two sequences based on matching keys. Equivalent to SQL `INNER JOIN`.

```csharp
var result = db.Orders
               .Join(
                   db.Customers,                     // Inner sequence
                   o => o.CustomerId,               // Outer key
                   c => c.Id,                       // Inner key
                   (o, c) => new                   // Result selector
                   {
                       o.OrderId,
                       c.Name,
                       o.Total
                   })
               .ToList();
// SQL: SELECT o.OrderId, c.Name, o.Total
//      FROM Orders o INNER JOIN Customers c ON o.CustomerId = c.Id
```

> 💡 In EF Core, prefer **Navigation Properties + `.Include()`** over manual `.Join()` — it's cleaner and less error-prone.

### `.GroupJoin()` — Left Outer Join (manual)

Produces a left join by matching each outer element with zero or more inner elements.

```csharp
var result = db.Customers
               .GroupJoin(
                   db.Orders,
                   c => c.Id,
                   o => o.CustomerId,
                   (c, orders) => new { c.Name, Orders = orders })
               .ToList();
```

### `.LeftJoin()` *(EF Core 10+)*

Native left join support added in EF Core 10, eliminating the verbose `GroupJoin + SelectMany` workaround.

```csharp
var result = db.Customers
               .LeftJoin(
                   db.Orders,
                   c => c.Id,
                   o => o.CustomerId,
                   (c, o) => new { c.Name, OrderTotal = o != null ? o.Total : 0 })
               .ToList();
// SQL: SELECT c.Name, COALESCE(o.Total, 0)
//      FROM Customers c LEFT JOIN Orders o ON c.Id = o.CustomerId
```

---

## 9. Grouping

### `.GroupBy(keySelector)`

Groups elements by a key. Translates to SQL `GROUP BY`.

```csharp
// Count users per country
var groups = db.Users
               .GroupBy(u => u.Country)
               .Select(g => new
               {
                   Country = g.Key,
                   Count   = g.Count()
               })
               .ToList();
// SQL: SELECT Country, COUNT(*) FROM Users GROUP BY Country
```

With multiple keys:

```csharp
var groups = db.Orders
               .GroupBy(o => new { o.CustomerId, o.Status })
               .Select(g => new
               {
                   g.Key.CustomerId,
                   g.Key.Status,
                   Total = g.Sum(o => o.Amount)
               })
               .ToList();
// SQL: GROUP BY CustomerId, Status
```

With `HAVING` (filter on group):

```csharp
// Only countries with more than 10 users
var result = db.Users
               .GroupBy(u => u.Country)
               .Where(g => g.Count() > 10)   // Translates to HAVING
               .Select(g => new { Country = g.Key, Count = g.Count() })
               .ToList();
// SQL: GROUP BY Country HAVING COUNT(*) > 10
```

---

## 10. Set Operations

These work like mathematical set operations. Operate on two sequences of the same type.

### `.Distinct()` / `.DistinctBy(keySelector)`

Removes duplicate elements.

```csharp
var countries = db.Users.Select(u => u.Country).Distinct().ToList();
// SQL: SELECT DISTINCT Country FROM Users

// DistinctBy — keep first occurrence per key (EF Core 7+)
var unique = db.Users.DistinctBy(u => u.Email).ToList();
```

### `.Union(other)` / `.UnionBy(other, keySelector)`

Returns all elements from both sequences, removing duplicates. Translates to SQL `UNION`.

```csharp
var allEmails = db.Users.Select(u => u.Email)
                        .Union(db.Admins.Select(a => a.Email))
                        .ToList();
// SQL: SELECT Email FROM Users UNION SELECT Email FROM Admins
```

### `.Concat(other)`

Returns all elements from both sequences, **keeping duplicates**. Translates to SQL `UNION ALL`.

```csharp
var all = db.Users.Select(u => u.Email)
                  .Concat(db.Admins.Select(a => a.Email))
                  .ToList();
// SQL: SELECT Email FROM Users UNION ALL SELECT Email FROM Admins
```

### `.Intersect(other)` / `.IntersectBy(other, keySelector)`

Returns only elements that appear in **both** sequences. Translates to SQL `INTERSECT`.

```csharp
var commonEmails = db.Users.Select(u => u.Email)
                           .Intersect(db.NewsletterSubscribers.Select(s => s.Email))
                           .ToList();
// SQL: SELECT Email FROM Users INTERSECT SELECT Email FROM NewsletterSubscribers
```

### `.Except(other)` / `.ExceptBy(other, keySelector)`

Returns elements in the **first** sequence that are **not** in the second. Translates to SQL `EXCEPT`.

```csharp
var nonSubscribers = db.Users.Select(u => u.Email)
                             .Except(db.NewsletterSubscribers.Select(s => s.Email))
                             .ToList();
// SQL: SELECT Email FROM Users EXCEPT SELECT Email FROM NewsletterSubscribers
```

---

## 11. Flattening

### `.SelectMany(collectionSelector)`

Projects each element to a collection, then flattens all collections into one sequence. Equivalent to a join that expands a nested list.

```csharp
// Get all tags from all blog posts in a flat list
var allTags = db.Posts
                .SelectMany(p => p.Tags)
                .Distinct()
                .ToList();

// With both parent and child in the result
var postTagPairs = db.Posts
                     .SelectMany(
                         p => p.Tags,
                         (post, tag) => new { post.Title, tag.Name })
                     .ToList();
```

---

## 12. Materialization 

These methods **execute the SQL query immediately** and load data into memory.

### `.ToList()` / `.ToListAsync()`

Executes the query and returns a `List<T>`. The most common materialization method.

```csharp
var users = await db.Users.Where(u => u.IsActive).ToListAsync();
```

### `.ToArray()` / `.ToArrayAsync()`

Executes the query and returns a `T[]`. Use when you need an array specifically.

```csharp
var ids = await db.Users.Select(u => u.Id).ToArrayAsync();
```

### `.ToDictionary(keySelector)` / `.ToDictionaryAsync()`

Executes the query and returns a `Dictionary<TKey, TElement>`. Keys must be unique.

```csharp
// Lookup user by ID — O(1) access after load
var userMap = db.Users.ToDictionary(u => u.Id);
var user    = userMap[42];

// With value selector
var emailMap = db.Users.ToDictionary(u => u.Id, u => u.Email);
```

### `.ToHashSet()`

Executes the query and returns a `HashSet<T>` — useful for fast `Contains` checks in memory.

```csharp
var activeIds = db.Users.Where(u => u.IsActive)
                        .Select(u => u.Id)
                        .ToHashSet();

bool isActive = activeIds.Contains(someId); // O(1) lookup
```

### `.AsAsyncEnumerable()`

Streams results one row at a time rather than loading everything into memory. Ideal for large datasets where you process rows as they arrive.

```csharp
await foreach (var user in db.Users.AsAsyncEnumerable())
{
    await ProcessUserAsync(user);
    // Each row is processed as it's received from DB — low memory footprint
}
```

---

## 13. Conversion & Casting

### `.AsQueryable()`

Converts an `IEnumerable` to `IQueryable`. Rarely needed unless building dynamic queries from in-memory collections.

```csharp
List<User> list = new List<User> { ... };
IQueryable<User> q = list.AsQueryable();
```

### `.AsEnumerable()`

Switches from server-side (`IQueryable`) to client-side (`IEnumerable`) execution. Use deliberately when EF cannot translate a C# function to SQL.

```csharp
var result = db.Users
               .Where(u => u.Age > 18)          // Runs on DB
               .AsEnumerable()                  // Switch to in-memory
               .Where(u => MyLocalFunc(u))      // Runs in C#
               .ToList();
```

### `.Cast<T>()`

Casts all elements to type `T`. Throws if any element cannot be cast.

```csharp
IQueryable<object> objects = db.Users;
IQueryable<User> users = objects.Cast<User>();
```

---

## 14. String & Value Helpers (EF-specific)

These are C# methods that EF Core knows how to translate into SQL functions.

### String methods

```csharp
// LIKE '%google%'
db.Blogs.Where(b => b.Url.Contains("google"))

// LIKE 'https%'
db.Blogs.Where(b => b.Url.StartsWith("https"))

// LIKE '%.com'
db.Blogs.Where(b => b.Url.EndsWith(".com"))

// UPPER(Name) = 'JOHN'
db.Users.Where(u => u.Name.ToUpper() == "JOHN")

// LEN(Name) > 5
db.Users.Where(u => u.Name.Length > 5)

// TRIM(Name)
db.Users.Select(u => u.Name.Trim())
```

### `EF.Functions` — SQL-specific functions

EF Core exposes SQL functions that have no direct C# equivalent via `EF.Functions`:

```csharp
// LIKE with wildcard (more flexible than .Contains)
db.Users.Where(u => EF.Functions.Like(u.Name, "%oh_"))

// Full-text search (SQL Server)
db.Articles.Where(a => EF.Functions.Contains(a.Body, "entity framework"))

// Date difference
db.Orders.Where(o => EF.Functions.DateDiffDay(o.OrderDate, DateTime.Now) < 30)
```

### Null checks

```csharp
// IS NULL
db.Users.Where(u => u.PhoneNumber == null)

// IS NOT NULL
db.Users.Where(u => u.PhoneNumber != null)

// Null coalescing → COALESCE(DisplayName, Name)
db.Users.Select(u => new { Name = u.DisplayName ?? u.Name })
```

---

## Quick Reference Cheat Sheet

| Category | Method | Deferred? | SQL Equivalent |
| :--- | :--- | :---: | :--- |
| Filter | `.Where()` | ✅ | `WHERE` |
| Filter | `.OfType<T>()` | ✅ | — |
| Project | `.Select()` | ✅ | `SELECT` |
| Project | `.SelectMany()` | ✅ | `JOIN` (flatten) |
| Order | `.OrderBy()` | ✅ | `ORDER BY ASC` |
| Order | `.OrderByDescending()` | ✅ | `ORDER BY DESC` |
| Order | `.ThenBy()` | ✅ | `, col ASC` |
| Order | `.ThenByDescending()` | ✅ | `, col DESC` |
| Page | `.Skip()` | ✅ | `OFFSET` |
| Page | `.Take()` | ✅ | `TOP` / `FETCH NEXT` |
| Aggregate | `.Count()` | 🔵 | `COUNT(*)` |
| Aggregate | `.Sum()` | 🔵 | `SUM()` |
| Aggregate | `.Average()` | 🔵 | `AVG()` |
| Aggregate | `.Min()` / `.Max()` | 🔵 | `MIN()` / `MAX()` |
| Quantifier | `.Any()` | 🔵 | `EXISTS` |
| Quantifier | `.All()` | 🔵 | `NOT EXISTS` |
| Quantifier | `.Contains()` | 🔵 | `IN (...)` |
| Element | `.First()` / `.FirstOrDefault()` | 🔵 | `TOP 1` |
| Element | `.Single()` / `.SingleOrDefault()` | 🔵 | `TOP 2` (validates) |
| Element | `.Last()` / `.LastOrDefault()` | 🔵 | `TOP 1 ORDER BY DESC` |
| Join | `.Join()` | ✅ | `INNER JOIN` |
| Join | `.LeftJoin()` | ✅ | `LEFT JOIN` |
| Group | `.GroupBy()` | ✅ | `GROUP BY` |
| Set | `.Distinct()` | ✅ | `DISTINCT` |
| Set | `.Union()` | ✅ | `UNION` |
| Set | `.Concat()` | ✅ | `UNION ALL` |
| Set | `.Intersect()` | ✅ | `INTERSECT` |
| Set | `.Except()` | ✅ | `EXCEPT` |
| Materialize | `.ToList()` | 🔵 | executes SQL |
| Materialize | `.ToArray()` | 🔵 | executes SQL |
| Materialize | `.ToDictionary()` | 🔵 | executes SQL |
| Convert | `.AsEnumerable()` | ✅ | switches to in-memory |
| Convert | `.AsQueryable()` | ✅ | switches to SQL |

> 🔵 = Executes immediately (materializes query) &nbsp; ✅ = Deferred (builds SQL expression tree)
