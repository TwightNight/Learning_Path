A learning roadmap to review the basics of C#.

# 🚀 Backend Developer Roadmap (.NET)

## 1) GitHub and Git workflow

**Goal:** work like a developer from day one.

### Learn
- git init, add, commit, push, pull, branch, merge
- commit message rules: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`
- small commits, one purpose per commit
- README, .gitignore, branch naming

### Practice
- create a repo
- make 5–10 tiny commits
- use branches for features
- write a simple README for every mini project

### Deliverable
- one GitHub repo with clean commit history

---

## 2) C# and .NET basics

**Goal:** understand syntax and core language features.

### Learn
- variables, types, operators
- if, switch, loops
- methods, parameters, return values
- arrays, strings, enums
- classes vs structs
- namespaces, access modifiers
- async/await basics

### Practice
- calculator
- student management console app
- string processing exercises

### Deliverable
- 1–2 console apps using clean C# syntax

---

## 3) OOP

**Goal:** write code with proper design.

### Learn
- class, object, constructor
- encapsulation, inheritance, polymorphism, abstraction
- interface vs abstract class
- composition over inheritance
- SOLID basics

### Practice
- build a simple order system
- model User, Admin, Product, Order
- use interfaces for services

### Deliverable
- a console app designed with OOP, not just functions

---

## 4) Data structures and collections

**Goal:** understand how data is stored and accessed.

### Learn
- IEnumerable, ICollection, IList
- List<T>, Dictionary<TKey,TValue>, HashSet<T>
- Stack<T>, Queue<T>
- array vs list
- basic complexity: O(1), O(n), O(log n)
- heap/priority queue if needed

### Practice
- sort/filter/search data manually
- implement stack-based expression evaluation
- build a to-do list using List<T>
- compare performance of List vs Dictionary

### Deliverable
- small exercises showing when to use each collection

---

## 5) Garbage Collection and memory basics

**Goal:** understand how .NET manages memory.

### Learn
- managed vs unmanaged memory
- stack vs heap
- reference vs value types
- generations: Gen 0, 1, 2
- boxing/unboxing
- IDisposable, using, cleanup

### Practice
- create objects and observe lifetime
- write code with using
- avoid unnecessary allocations

### Deliverable
- explain why an object is collected and when Dispose is needed

---

## 6) Database and SQL

**Goal:** store and query data correctly.

### Learn
- tables, primary key, foreign key
- normalization
- CRUD SQL
- SELECT, JOIN, GROUP BY, WHERE, ORDER BY
- indexing basics
- transactions

### Practice
- design a database for users, products, orders
- write queries for searching and joining data
- create relationships between tables

### Deliverable
- a small relational database with meaningful schema

---

## 7) EF Core / ORM

**Goal:** connect C# to database cleanly.

### Learn
- DbContext, DbSet
- entity configuration
- migrations
- tracking vs no-tracking
- eager/lazy loading
- relationships
- LINQ to Entities

### Practice
- build CRUD with EF Core
- create migrations
- query with Include, Where, Select
- map DTOs to entities

### Deliverable
- a working data layer with EF Core

---

## 8) RESTful API and Web API

**Goal:** expose your backend properly.

### Learn
- HTTP methods: GET, POST, PUT, PATCH, DELETE
- status codes: 200, 201, 204, 400, 401, 403, 404, 500
- route design
- request/response DTOs
- validation
- model binding
- middleware
- exception handling
- dependency injection

### Practice
- create CRUD endpoints
- validate input
- return proper status codes
- handle errors consistently

### Deliverable
- a clean ASP.NET Core Web API

---

## 9) Docker / dev container

**Goal:** run your app in a reproducible environment.

### Learn
- Dockerfile
- docker compose
- image vs container
- ports, environment variables
- containerizing API + database

### Practice
- dockerize your Web API
- run SQL Server/PostgreSQL in Docker
- connect API to database through compose

### Deliverable
- the whole project runs with one `docker compose up`

---

## 10) Final integration project

**Goal:** combine everything into one real project.

### Build something like:
- Book Store API
- Task Management API
- Student Management API
- Booking system API

### Must include:
- GitHub with good commits
- C# OOP design
- collections usage
- SQL database
- EF Core
- RESTful API
- Docker
- basic GC/memory awareness

---

## 📌 Recommended order
1. GitHub  
2. C# basics  
3. OOP  
4. Data structures / collections  
5. Garbage collection  
6. Database / SQL  
7. EF Core  
8. Web API / RESTful  
9. Docker  
10. Final project  