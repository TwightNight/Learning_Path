***

# 🚀 Object-Oriented Programming (OOP) in C#

> **Object-Oriented Programming (OOP)** is a programming paradigm built on the concept of **objects**. Another definition of OOP is a programming approach based on the architecture of **classes** and **objects**.

## 🏗️ 1. Classes and Objects

*   **Class (Blueprint):** Considered a blueprint or template. It defines the data (variables/fields) and behaviors (methods) that objects of that class will possess.
*   **Object (Instance):** A specific, concrete entity created from a class at runtime. 
    *   *Example:* "Person" is a class, while a specific person named "John Doe" is an object.

---

## 🏛️ 2. The Four Pillars of OOP

OOP relies on four core principles to manage complex software systems:

1.  **Encapsulation:** Bundling data and the methods that operate on that data within a single class, while restricting direct access from the outside. This protects data integrity and allows internal logic to change without breaking the external code that uses it.
2.  **Inheritance:** Allows a new class (child/derived class) to reuse and extend the members (data and behavior) of an existing class (parent/base class). This establishes an **"is-a"** relationship. 
    *   *Example:* "An Admin *is a* User."
3.  **Polymorphism:** Allows a method to have the same name but behave differently depending on the specific object invoking it. This is typically achieved through method **overriding** in child classes.
4.  **Abstraction:** Focusing on **"what an object does"** rather than **"how it does it."** It hides complex implementation details and only exposes essential features to the user.

---

## 🧩 3. Other Core Components

*   **Constructor:** A special method called automatically when an object is instantiated, designed to put the object into a valid initial state.
*   **Properties:** Often called "smart fields," they use `get` and `set` accessors to control how data is read and written, ensuring data safety within the class.
*   **Access Modifiers:** Keywords like `public`, `private`, and `protected` that define the boundaries and visibility of class members.

---

## 🛠️ 4. Constructors in C#

In C#, a Constructor is a special method executed automatically as soon as an instance of a class is created. Its main purpose is to initialize data members.

### 4.1. Parameterless Constructor
If no constructor is defined, the compiler automatically generates a default parameterless constructor.

```csharp
public class Taxi 
{
    public bool isGroupSize;
    
    public Taxi() 
    {
        // Assigning a default value when the object is created
        isGroupSize = true; 
    }
}
```

### 4.2. Parameterized Constructor
Allows you to pass data directly into the object at the time of creation, making the initial state setup flexible.

```csharp
public class Employee 
{
    public int id;
    public string name;
    
    public Employee(int id, string name) 
    {
        this.id = id;       // 'this' refers to the current class instance
        this.name = name;
    }
}
```

### 4.3. Constructor Chaining (`this`)
Use the `this()` keyword to call another constructor within the same class, avoiding code duplication.

```csharp
public class BankAccount 
{
    // Calls the 3-parameter constructor below, passing 0 as the default minimum balance
    public BankAccount(string name, decimal initialBalance) 
        : this(name, initialBalance, 0) 
    { 
    }

    public BankAccount(string name, decimal initialBalance, decimal minimumBalance) 
    {
        // Centralized initialization logic goes here
    }
}
```

### 4.4. Base Constructor (`base`)
In inheritance, a child class uses the `base()` keyword to pass parameters up to the parent class's constructor, ensuring the parent is initialized first.

```csharp
public class InterestEarningAccount : BankAccount 
{
    // Passing data up to the base class
    public InterestEarningAccount(string name, decimal initialBalance) 
        : base(name, initialBalance) 
    {
        // Child class specific logic (if any)
    }
}
```

### 4.5. Primary Constructor (C# 12)
A modern feature that shortens code by declaring parameters directly in the class header.

```csharp
// Parameters declared directly in the header
public class Person(string name, int age) 
{
    public string Name => name; 
    public int Age => age;
}
```

### 4.6. Static Constructor
Used to initialize static data members. It runs **only once** before any instance is created or any static member is accessed.
> **Note:** It cannot have parameters and cannot be called directly.

---

## 🔒 5. Encapsulation & Properties

Encapsulation and Properties work hand-in-hand in C# to protect data and create clean, safe programming interfaces.

### 5.1. Encapsulation Details
Encapsulation uses **Access Modifiers** to control visibility:
*   `private`: Accessible only within the same class (highest security, typically used for data fields).
*   `public`: Accessible from anywhere within the project.
*   `protected`: Accessible within the same class and its derived (child) classes.

**Benefits:**
*   **Data Integrity:** Prevents invalid value assignments (e.g., negative prices or empty passwords).
*   **Maintainability:** Internal logic can be changed without breaking external code.
*   **Security:** Hides complex implementation details.

### 5.2. Properties Details
Properties act as **"smart fields."** They look like public fields but are actually special methods called **accessors**.

**Types of Accessors:**
*   `get`: Invoked when the property value is read.
*   `set`: Invoked when assigning a new value. The keyword `value` represents the data being assigned.
*   `init` *(C# 9+)*: Allows assignment only during object initialization/construction, enabling the creation of immutable objects.

**Implementations:**
*   **Auto-implemented Properties:** The compiler generates a hidden backing field. 
    *   *Syntax:* `public string Name { get; set; }`
*   **Manual Properties:** Allows adding validation logic in the `set` block or calculation logic in the `get` block.
*   **Required Properties *(C# 12+)*:** Uses the `required` keyword to force users to assign a value upon initialization.
*   **Field-backed Properties *(C# 14+)*:** Uses the `field` keyword to access the backing field directly without manually declaring a private variable.

---

## 🧬 6. Inheritance

Inheritance allows you to define a new derived class based on an existing base class. The child class automatically inherits members from the parent class, promoting code reuse.

### 6.1. Basic Concepts & Rules
*   **"Is-a" Relationship:** E.g., a Dog *is an* Animal.
*   **Single Inheritance:** C# only allows a class to inherit directly from **one** parent class. However, inheritance is transitive (If A inherits B, and B inherits C, A gets C's members).
*   **`System.Object`:** Every class in C# implicitly inherits from `System.Object`, granting basic methods like `ToString()` and `Equals()`.
*   **Non-inherited Members:** Constructors, finalizers, and static constructors are **not** inherited. The child class must define its own constructors.

### 6.2. Key Keywords in Inheritance
*   `virtual`: Used in the parent class to mark a method/property that **can** be overridden. It provides a default implementation.
*   `override`: Used in the child class to replace the implementation of a `virtual` or `abstract` method.
*   `base`: Used to access parent class members or call the parent constructor (`: base()`).
*   `abstract`: Marks a class that cannot be instantiated, or a method without a body that **must** be overridden by child classes.
*   `sealed`: Prevents a class from being inherited or a method from being overridden any further.

### 6.3. The `protected` Modifier
In inheritance, `protected` allows members to be visible to the containing class and its child classes, while remaining hidden from the outside world.

### 6.4. Order of Object Initialization
When an instance of a child class is created, C# follows this sequence:
1.  Assigns default values (`0`, `null`) to fields.
2.  Executes field initializers of the child class.
3.  Executes field initializers of the parent class.
4.  Executes the parent class's constructor (top-down from `System.Object`).
5.  Executes the child class's constructor.


---
### Reference: https://www.youtube.com/watch?v=pXcMdI3LVEE&list=PL33lvabfss1zRgaWBcC__Bnt5AOSRfU71&index=2, https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop