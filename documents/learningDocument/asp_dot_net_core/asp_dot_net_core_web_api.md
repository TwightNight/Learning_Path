# ASP.NET CORE WEB API

## Part 1 — How do the Internet & Web work?

### 1.1 Client and Server
Everything on the internet revolves around the **Client-Server** model:
*   **Client**: The one sending the request — e.g., your web browser, a mobile app, or Postman.
*   **Server**: The one receiving the request, processing it, and returning the result.

When you type `google.com` into your browser, the browser (client) sends a request to Google's computers (server). The server processes it and sends back an HTML page for your browser to display.

### 1.2 What is HTTP?
**HTTP (HyperText Transfer Protocol)** is the "language" that clients and servers use to communicate over the Internet. Every conversation consists of two parts:
*   **Request**: Sent by the client to the server.
*   **Response**: Sent back by the server to the client.

*Example of a simplified HTTP Request:*
```http
GET /products HTTP/1.1
Host: api.myshop.com
Accept: application/json
```
*And the corresponding Response:*
```http
HTTP/1.1 200 OK
Content-Type: application/json[{"id": 1, "name": "Laptop"}, {"id": 2, "name": "Mouse"}]
```

---

## Part 2 — What is an API?

An **API (Application Programming Interface)** is a "gateway" provided by one piece of software so that other software can communicate with it.

Imagine dining at a restaurant:
*   **The Menu** = API (the list of commands/requests you can make).
*   **You (Customer)** = Client.
*   **The Kitchen** = Server (where the actual processing and cooking happen).
*   **The Waiter** = HTTP Protocol (carries your request to the kitchen and brings the food back).

A **Web API** is specifically an API that operates over HTTP, allowing different systems (Web, Mobile, IoT) written in any language to talk to each other.

---

## Part 3 — What is REST?

### 3.1 The Problem Before REST
In the past, developers created URLs based on their own preferences:
```text
/getProduct?id=1
/createNewProduct
/deleteProductById?id=1
```
This created chaos. Every API had its own rules, making them extremely hard to learn and maintain.

### 3.2 REST and its 6 Core Constraints
**REST (Representational State Transfer)** is a **set of architectural design principles** (not a technology) proposed by Roy Fielding in 2000. For a system to be considered RESTful, it must adhere to:

1.  **Client-Server:** Complete separation between the user interface logic (Client) and data storage/processing logic (Server).
2.  **Stateless:** Every request from the client must contain all the information the server needs to process it. The server does not "remember" previous requests. (e.g., Once logged in, every subsequent request must include an Auth Token).
3.  **Cacheable:** The server must indicate whether the response can be cached by the client to reduce server load.
4.  **Uniform Interface:** Use a standard, uniform way to access resources (This is the most critical constraint).
5.  **Layered System:** The client doesn't need to know if it's talking directly to the end server or passing through intermediaries (Load Balancers, Proxies, etc.).
6.  **Code on Demand (Optional):** The server can send executable code (like JavaScript) to the client.

---

## Part 4 — RESTful API: Practical Application

A **RESTful API** is a Web API designed in strict compliance with the REST principles mentioned above.

### 4.1 The "Resource" Mindset
The heart of RESTful design revolves around **nouns**. Instead of thinking "What action am I performing?", you think "What resource am I interacting with?".

*URL Naming Rules:* Use plural nouns, DO NOT use verbs.
*   ❌ Bad: `POST /createProduct` or `GET /getAllUsers`
*   ✅ Good: `POST /products` or `GET /users`

### 4.2 HTTP Methods (Verbs)
The action is determined by the HTTP Method:
*   `GET /products` ➔ Retrieve a list of products.
*   `GET /products/5` ➔ Retrieve the product with ID = 5.
*   `POST /products` ➔ Create a new product.
*   `PUT /products/5` ➔ Fully update the product with ID = 5.
*   `PATCH /products/5` ➔ Partially update (specific fields) the product with ID = 5.
*   `DELETE /products/5` ➔ Delete the product with ID = 5.

*(Note: GET, PUT, and DELETE are **Idempotent** — whether you call them once or 100 times, the system state remains the same. POST is not; calling it 100 times creates 100 new records).*

### 4.3 HTTP Status Codes
The server communicates the outcome to the client using standard codes:
*   **2xx (Success):** `200 OK` (General success), `201 Created` (Successfully created), `204 No Content` (Successfully deleted/updated, no data returned).
*   **4xx (Client Error):** `400 Bad Request` (Invalid data sent), `401 Unauthorized` (Not logged in), `403 Forbidden` (Logged in but lacks permissions), `404 Not Found` (Wrong URL or ID doesn't exist).
*   **5xx (Server Error):** `500 Internal Server Error` (The server's code crashed).

### 4.4 Advanced RESTful Techniques (Must-know for the workplace)
*   **Pagination, Filtering & Sorting:** Use Query Strings to prevent crashing the server when there are millions of records.
    *   `GET /products?page=1&pageSize=20&category=laptop&sortBy=price&order=desc`
*   **API Versioning:** When upgrading your app, you must keep the old API for users who haven't updated their mobile apps.
    *   `GET /api/v1/products` (Old) and `GET /api/v2/products` (New).
*   **HATEOAS:** The highest maturity level of REST. The response includes hypermedia links guiding the client on the next possible actions.
    *   *Example:* After fetching a user, the API returns a link like `{"rel": "change-password", "href": "/users/5/password"}`.

---

## Part 5 — Mastering ASP.NET Core Web API

After internalizing the RESTful mindset, we use **ASP.NET Core (C#)** to bring it to life.

### 5.1 Why use ASP.NET Core?
*   Cross-platform (runs on Windows, Linux, macOS, Docker).
*   Top-tier performance worldwide among Web Frameworks.
*   Standardized ecosystem: Built-in Dependency Injection, Swagger (API documentation), and robust Auth.

### 5.2 Two Core Concepts to Memorize
1.  **Dependency Injection (DI):** Do not use the `new` keyword to instantiate services (e.g., `new ProductService()`). ASP.NET Core has an IoC Container that automatically "injects" services where needed. You must understand its 3 lifetimes: *Transient, Scoped, Singleton*.
2.  **DTOs (Data Transfer Objects):** NEVER return your Entity (the class mapped to the Database) directly out of the API. You must create DTOs to hide sensitive data (like passwords) and prevent infinite loop recursion (Circular References).

### 5.3 The Request Lifecycle in ASP.NET Core
Understanding this helps you know exactly where your code executes:
`Client Request` ➔ `Middleware Pipeline (Auth, CORS, Logging...)` ➔ `Routing (Finds the Controller)` ➔ `Model Binding (Parses JSON to C# objects)` ➔ `Validation (Checks data constraints)` ➔ **`Action Method (Your Logic)`** ➔ `Response`.

### 5.4 The Architecture Battle: API Controllers vs. Minimal APIs
ASP.NET Core provides 2 ways to build APIs.

#### Approach 1: API Controllers (Traditional, Classic, OOP-based)
Uses classes, inherits from `ControllerBase`, and relies heavily on Attributes.

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service; // DI applied here

    public ProductsController(IProductService service) 
    { 
        _service = service; 
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id) 
    {
        var product = _service.Get(id);
        if (product == null) return NotFound();
        return Ok(product);
    }
}
```

#### Approach 2: Minimal APIs (Modern, Lightweight, Functional-based - since .NET 6)
Written directly in `Program.cs`, completely eliminating bulky Controller folders.

```csharp
// Inside Program.cs
var app = builder.Build();

app.MapGet("/api/v1/products/{id}", (int id, IProductService service) => 
{
    var product = service.Get(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.Run();
```

#### Comparison Table & Practical Advice:

| Criteria | API Controllers | Minimal APIs |
| :--- | :--- | :--- |
| **Nature** | Based on MVC model, uses Attributes. | Maps URLs directly via Lambda Expressions. |
| **Code Volume** | Lots of boilerplate code. | Extremely concise and minimal. |
| **Performance** | Fast. | **Blazing Fast** (bypasses the MVC pipeline). |
| **Validation** | Automatically returns 400 if the model is invalid. | Requires manual setup via third-party libraries (e.g., FluentValidation). |
| **Best Fit For** | Monolithic, Large Enterprise projects, Clean Architecture. | Small Microservices, Serverless, Vertical Slice Architecture. |

---