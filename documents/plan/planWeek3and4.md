# 🗓️ ASP.NET Core Learning Plan

---

## 🟡 Week 3: ASP.NET Core Web API

### Day 15: Intro to ASP.NET Core Web API
- [x] Understand HTTP Methods: GET, POST, PUT, DELETE
- [x] Understand Status Codes: 200, 201, 400, 404, 500
- [x] Differentiate API Controllers vs Minimal APIs
- [x] Run `dotnet new webapi`, explore the default boilerplate
- [x] Create a `ProductsController`
- [x] Add an `[HttpGet]` that returns a hardcoded list of products
- [x] Test the API via Swagger or Postman

**Deliverable:** A running Web API tested successfully via Swagger/Postman

---

### Day 16: Building CRUD Endpoints
- [x] Learn route parameters: `[FromRoute]`
- [x] Learn query parameters: `[FromQuery]`
- [x] Learn body payload: `[FromBody]`
- [x] Implement full CRUD in `ProductsController`
- [x] Integrate with EF Core `DbContext` from Week 2
- [x] Test `POST /api/products` saving data to the DB
- [x] Test `GET`, `PUT`, `DELETE` against a real database

**Deliverable:** Fully functional RESTful endpoints interacting with a database

---

### Day 17: Validation & Model Binding
- [x] Learn Data Annotations: `[Required]`, `[MaxLength]`, `[Range]`
- [x] Understand how ModelState validation works
- [x] Create a `CreateProductDto` with validation rules
- [x] Add rules: `Price > 0`, `Name` is required
- [x] Test sending bad data via Postman, observe the `400 Bad Request` response
- [x] Customize error messages in the response body

---

### Day 18: Dependency Injection & Middleware
- [ ] Learn DI Lifetimes: Transient, Scoped, Singleton
- [ ] Understand when to use each lifetime
- [ ] Extract database logic into an `IProductService`
- [ ] Register the service as Scoped in `Program.cs`
- [ ] Write a custom Exception Handling Middleware
- [ ] Middleware catches unhandled errors and returns a clean JSON `500` response
- [ ] Verify middleware works correctly end-to-end

**Deliverable:** A structured, layered API utilizing Dependency Injection

---

### Day 19: Week 3 Review & Postman Testing
- [ ] Review RESTful API design principles
- [ ] Write Postman tests for all endpoints (GET, POST, PUT, DELETE)
- [ ] Verify the full request flow: Request → Middleware → Controller → Service → DB
- [ ] Refactor any messy code, apply consistent naming conventions
- [ ] Commit everything with conventional commits

---

## 🟠 Week 4: ASP.NET Core MVC

### Day 20: Intro to MVC Pattern & Project Setup
- [ ] Understand the MVC pattern: Model – View – Controller
- [ ] Differentiate MVC vs Web API (server-side rendering vs JSON)
- [ ] Run `dotnet new mvc`, explore the project structure
- [ ] Understand the roles of `Program.cs`, `Controllers/`, `Views/`, `Models/`
- [ ] Understand default routing: `{controller}/{action}/{id?}`
- [ ] Create a `HomeController` with an `Index` action returning a simple View

**Deliverable:** Your first MVC web page running in the browser

---

### Day 21: Razor Views & Layouts
- [ ] Learn Razor syntax: `@model`, `@foreach`, `@if`, `@Html.*`
- [ ] Understand `_Layout.cshtml` and `_ViewStart.cshtml`
- [ ] Create a shared layout with a navbar and footer
- [ ] Use `ViewBag` and `ViewData` to pass data into Views
- [ ] Create a `Partial View` for a reusable component (e.g. product card)
- [ ] Use Tag Helpers: `asp-controller`, `asp-action`, `asp-for`

---

### Day 22: Controllers, Actions & Forms
- [ ] Understand Action Result types: `View()`, `RedirectToAction()`, `Json()`, `NotFound()`
- [ ] Create a `ProductsController` with actions: `Index`, `Details`, `Create`, `Edit`, `Delete`
- [ ] Handle HTTP GET (display form) and HTTP POST (submit form)
- [ ] Use `[HttpGet]` and `[HttpPost]` attributes correctly
- [ ] Check `ModelState.IsValid` inside POST actions
- [ ] Use `TempData` for success/error flash messages

---

### Day 23: Model Binding, Validation & ViewModels
- [ ] Differentiate Domain Model vs ViewModel vs DTO
- [ ] Create `CreateProductViewModel` and `EditProductViewModel`
- [ ] Add Data Annotations validation on ViewModels
- [ ] Display validation errors in Razor Views using `asp-validation-for`
- [ ] Use `asp-validation-summary` to show a full error summary
- [ ] Test submitting invalid data and observe client + server validation

---

### Day 24: Entity Framework Core in MVC
- [ ] Inject `DbContext` into Controllers via Dependency Injection
- [ ] Implement `Index`: fetch list of products from DB
- [ ] Implement `Details`: fetch by ID, return `404` if not found
- [ ] Implement `Create` POST: save new record to DB
- [ ] Implement `Edit` POST: update existing record in DB
- [ ] Implement `Delete`: remove from DB and redirect to Index
- [ ] Handle `DbUpdateConcurrencyException`

**Deliverable:** Full CRUD with a database through a web UI

---

### Day 25: Authentication & Authorization (ASP.NET Identity)
- [ ] Add ASP.NET Core Identity to the project
- [ ] Create and apply migrations for Identity tables
- [ ] Scaffold Login and Register pages
- [ ] Add `[Authorize]` attribute to protect routes
- [ ] Add `[AllowAnonymous]` to public-facing pages
- [ ] Display the logged-in user's name in the Layout
- [ ] Test the full flow: Register → Login → Access protected page → Logout

---

### Day 26: UI Polish & Week 4 Review
- [ ] Add Bootstrap or Tailwind CSS to the Layout
- [ ] Improve the product list and form UI
- [ ] Add simple pagination using `.Skip().Take()`
- [ ] Add search/filter by product name
- [ ] Review all MVC code, refactor where needed
- [ ] Commit everything with conventional commits

**Deliverable:** A complete MVC web app with authentication

---

## 🔵 Week 5: Docker & Containerization

### Day 27: Docker Fundamentals
- [ ] Understand Images vs Containers
- [ ] Learn Dockerfile syntax: `FROM`, `WORKDIR`, `COPY`, `RUN`, `ENTRYPOINT`
- [ ] Install Docker Desktop
- [ ] Write a `Dockerfile` for your ASP.NET Core Web API
- [ ] Build the image: `docker build -t my-api .`
- [ ] Run the container: `docker run -p 8080:80 my-api`
- [ ] Verify the API is accessible at `localhost:8080`

**Deliverable:** Your API running inside a Docker container

---

### Day 28: Docker Compose & Multi-container Setup
- [ ] Learn `docker-compose.yml` syntax
- [ ] Understand environment variables in Docker Compose
- [ ] Understand networking between containers
- [ ] Create a `docker-compose.yml` with two services: Web API + SQL Server/PostgreSQL
- [ ] Pass the connection string via environment variables
- [ ] Test `docker-compose up -d` to spin up the full stack
- [ ] Verify the API connects to the DB container successfully

**Deliverable:** Full app + database running with `docker-compose up -d`

---

### Day 29: Dockerize the MVC App
- [ ] Write a separate `Dockerfile` for your MVC project
- [ ] Add the MVC app as a third service in `docker-compose.yml`
- [ ] Ensure MVC app and Web API can communicate within the compose network
- [ ] Test the full stack: MVC → API → DB all running in containers
- [ ] Handle static files (`wwwroot`) correctly inside the container

---

### Day 30: Final Review & Documentation
- [ ] Write a `README.md` for the Web API project (setup, run with Docker)
- [ ] Write a `README.md` for the MVC project (setup, run with Docker)
- [ ] Document all environment variables used in `docker-compose.yml`
- [ ] Run the complete stack from scratch (`docker-compose up -d`) and verify everything works
- [ ] Push all images to Docker Hub (optional)
- [ ] Final commit with a clean git history

**Deliverable:** Both projects fully containerized, documented, and production-ready