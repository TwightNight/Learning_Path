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
Here is the translated and beautifully formatted Markdown document in English, maintaining the style of the previous sections.

***

## 🎭 7. Polymorphism

In Object-Oriented Programming (OOP) with C#, **Polymorphism** means "many forms." It allows an object or a method to exhibit different behaviors depending on the specific context. This mechanism enables a single interface to represent multiple underlying implementations.

### 7.1. Types of Polymorphism
Polymorphism is divided into two main categories based on when the system decides which method to execute:

*   **Compile-time Polymorphism (Static Polymorphism):** Achieved through **Method Overloading** or **Operator Overloading**. The compiler determines which method to call based on the method signature (number and data types of parameters) as soon as the code is compiled.
*   **Runtime Polymorphism (Dynamic Polymorphism):** Achieved through **Method Overriding** and **Upcasting**. The decision of which logic to execute is made while the program is running, based on the *actual* type of the object in memory, not the type of the reference variable.

### 7.2. Core Keywords in Dynamic Polymorphism
To implement runtime polymorphism, C# uses a specific set of keywords to establish relationships between parent and child classes:

*   **`virtual`**: Used in a parent class to mark a method, property, or event that can be redefined in a child class. A `virtual` method typically provides a default implementation.
*   **`override`**: Used by a child class to provide a new, specific implementation for a method marked as `virtual` or `abstract` in the parent class.
*   **`abstract`**: Used to declare incomplete classes or methods. An `abstract` method has no body and **forces** child classes to override it to provide the execution logic.
*   **`sealed`**: The exact opposite of polymorphism. This keyword prevents a class from being inherited or stops a child class from further overriding an already overridden method, thereby protecting critical logic.

### 7.3. Upcasting and Dynamic Dispatch
This is the "heart" of polymorphism in C#:
*   **Upcasting:** The act of storing a child class object in a parent class reference variable. 
    *   *Example:* `Animal myDog = new Dog();`
*   **Dynamic Dispatch:** When a `virtual` method is called on a parent reference variable, the Runtime (CLR) checks the actual type of the stored object and invokes the deeply `overridden` method in the derived class. If the child class hasn't overridden it, the parent class's method is executed.

### 7.4. Overriding vs. Method Hiding
C# provides the **`new`** keyword to hide parent class methods instead of overriding them. The key differences are:
*   **Override:** Participates in dynamic polymorphism. If invoked through a parent variable, the child class's method is still executed.
*   **New (Hiding):** Does **not** participate in dynamic polymorphism. If invoked through a parent variable, the parent class's method is executed instead of the child's.

### 7.5. Benefits of Polymorphism
*   **Flexibility:** Allows the same block of code to operate on multiple types of objects, making it easy to extend the system without modifying existing code.
*   **Code Reusability:** Common behaviors are defined in the parent class, while highly specific behaviors are refined in the child classes.
*   **Decoupling:** Encourages system design based on abstract classes or interfaces rather than concrete implementations.

> **Real-world Example:** A `Shape` class has a `virtual CalculateArea()` method. Classes like `Circle`, `Rectangle`, and `Square` inherit from `Shape` and `override` this method with their own mathematical formulas. When you iterate through a list of shapes (`List<Shape>`), you simply call `CalculateArea()`. You don't need to check what specific shape it is; the system automatically calculates the correct area for each.

---

## 💡 8. Abstraction

**Abstraction** is one of the four main pillars of OOP in C#. It focuses on hiding complex internal implementation details and exposing only the essential features and behaviors of an object to the user. Instead of describing *how* an object works in detail, abstraction focuses purely on **what** the object does.

### 8.1. Abstract Class
An abstract class serves as a "blueprint" or template for other classes to inherit from.
*   **Declaration:** Marked with the `abstract` keyword.
*   **Cannot be instantiated:** You **cannot create an object directly** from an abstract class (e.g., `new AbstractClass()` will throw an error).
*   **Components:** An abstract class can contain both concrete methods (with implementation bodies) and abstract methods (without bodies).
*   **Constructors:** Abstract classes can still have constructors and instance variables. These are used to initialize the state for child classes via the `base()` keyword.

### 8.2. Abstract Method
These are methods that define a behavior but lack a body (implementation logic).
*   **Mandatory Override:** Any non-abstract child class inheriting from the parent **must** provide a concrete implementation for these methods using the `override` keyword.
*   **Role:** It establishes a strict "contract," ensuring that all derived classes possess certain behaviors, even if their internal execution differs.

### 8.3. Benefits of Abstraction
*   **Complexity Management:** Helps developers focus on the essential properties and interactions of entities within the system rather than getting bogged down by details.
*   **Decoupling:** Other parts of the program depend on abstract concepts rather than concrete classes, making the system much easier to scale and maintain.
*   **Code Reusability:** Allows sharing common logic in the parent class while still permitting strict specialization in child classes.

---

### 8.4. Practical Example

#### Example: Animal System
The `Animal` class is an abstract concept because you cannot have a generic "animal" in the real world; it must be a specific species.

```csharp
public abstract class Animal 
{
    // Abstract method: All animals make a sound, but HOW they sound differs.
    // Notice there are no curly braces { } here.
    public abstract void MakeSound(); 
}

public class Dog : Animal 
{
    // The child class MUST override and provide the implementation.
    public override void MakeSound() => Console.WriteLine("Woof!");
}

public class Cat : Animal 
{
    public override void MakeSound() => Console.WriteLine("Meow!");
}
```

---
### Reference: https://www.youtube.com/watch?v=pXcMdI3LVEE&list=PL33lvabfss1zRgaWBcC__Bnt5AOSRfU71&index=2, https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop