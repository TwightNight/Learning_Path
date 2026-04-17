### **Advanced C# Study Guide**

#### **1. Access Modifiers**
Access modifiers are keywords used to specify the declared accessibility of a member or a type. C# provides six accessibility levels:

*   **public**: Access is not limited; the member can be accessed by any other code in the same assembly or another assembly that references it.
*   **private**: Access is limited to the containing type (class or struct).
*   **internal**: Access is limited to the current assembly (the project file).
*   **protected**: Access is limited to the containing class or types derived from that class.
*   **protected internal**: Access is limited to the current assembly **or** types derived from the containing class in another assembly.
*   **private protected**: Access is limited to the containing class **or** types derived from the containing class within the current assembly.

#### **2. Namespaces**
Namespaces provide a hierarchical way to organize C# programs and libraries, grouping related types and preventing naming collisions.
*   **Organization**: A namespace declaration puts your types into an organized structure, often mirroring the folder structure of your project.
*   **Using Directives**: These allow you to use types by their simple names instead of fully qualified names (e.g., writing `Console` instead of `System.Console`).
*   **File-scoped Namespaces**: Introduced to reduce nesting, this syntax uses a semicolon after the namespace declaration and applies to the entire file.

#### **3. Classes vs. Structs**
Choosing between a class (reference type) and a struct (value type) is a fundamental design decision.

| Feature | Class | Struct |
| :--- | :--- | :--- |
| **Type** | Reference type. | Value type. |
| **Memory** | Allocated on the **Heap** and garbage-collected. | Allocated on the **Stack** or inline within containing types. |
| **Assignment** | Copies the **reference**; both variables point to the same object. | Copies the **entire value**; variables have their own independent copy. |
| **Inheritance** | Supports single inheritance and polymorphism. | Does not support user-specified inheritance; implicitly inherits from `System.ValueType`. |

**Recommendation**: Define a struct only if the type represents a single value, is under 16 bytes in size, is **immutable**, and will not be boxed frequently.

#### **4. Enums**
An enum (enumeration) is a value type that defines a set of named constants mapping to underlying numeric values.
*   **Purpose**: Enums replace "magic numbers" with descriptive names, making code self-documenting and easier to maintain.
*   **The `[Flags]` Attribute**: This attribute allows an enum to be treated as a **bit field**, meaning a single variable can store a combination of multiple values (e.g., User Permissions) using bitwise operations like OR (`|`) and AND (`&`).

#### **5. Async/Await & Task**
Asynchronous programming allows methods to yield control without blocking threads, keeping applications responsive.
*   **Task**: Represents an ongoing operation. `ValueTask` is a struct alternative used to reduce memory allocation if the result is often available synchronously.
*   **Async/Await**: The `async` keyword marks a method for asynchronous execution, while `await` pauses the method until the task completes, releasing the current thread back to the thread pool.
*   **Common Mistakes**:
    *   **Avoid `async void`**: This should only be used for event handlers because exceptions in `async void` methods cannot be caught by the caller and may crash the application.
    *   **Avoid `.Wait()` or `.Result`**: These are blocking calls that can freeze the UI thread and lead to **deadlocks** in synchronization contexts.
    *   **CancellationToken**: Always include this in async methods to allow the caller to gracefully cancel long-running operations.

---
### Reference:
**Access modifiers**: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/access-modifiers
**Choosing Between Class and Struct**: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
**C# Advanced**: https://learn.microsoft.com/en-us/shows/c-advanced/
**Namespaces and using directives**: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/namespaces
**Enum**: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/enums