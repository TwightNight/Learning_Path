
# C# Syntax & Control Flow

## 1. Variables
Variables are containers for storing data values.

**Types of Variables:**
- `int`: stores integers (whole numbers), without decimals, such as `123` or `-123`. Stores whole numbers from -2,147,483,648 to 2,147,483,647.
- `long`: Stores whole numbers from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807.
- `float`: Stores fractional numbers. Sufficient for storing 6 to 7 decimal digits.
- `double`: stores floating point numbers, with decimals, such as `19.99` or `-19.99`. Stores fractional numbers. Sufficient for storing 15 decimal digits
- `char`: stores single characters, such as `'a'` or `'B'`. Char values are surrounded by single quotes.
- `string`: stores text, such as `"Hello World"`. String values are surrounded by double quotes.
- `bool`: stores values with two states: `true` or `false`.

---

## 2. Constants
- You can add the `const` keyword in front of the variable type if you don't want others (or yourself) to overwrite existing values.
- The `const` keyword is useful when you want a variable to always store the same value, so that others (or yourself) won't mess up your code. An example that is often referred to as a constant, is `PI` (3.14159...).
- **Note:** You cannot declare a constant variable without assigning the value. If you do, an error will occur: *A const field requires a value to be provided.*

---

## 3. Value Types vs Reference Types
In C#, data is divided into two main types based on how it is stored in memory:

### Value Types
Store values ​​directly in the **Stack** memory region. Fast access speed.
- **Examples:** `int`, `double`, `float`, `char`, `bool`, `struct`.
- **Characteristic:** When assigning `a = b`, it creates a complete copy. Changing `a` does not affect `b`.

### Reference Types
Store memory addresses (pointers) in the **Stack** memory region, while the actual data is in the **Heap**.
- **Examples:** `string`, `class`, `array`, `object`.
- **Characteristic:** When assigning `a = b`, both point to the same memory region. Changing the data inside `a` will also change the data inside `b`.

---

## 4. Conditions
C# has the following conditional statements:
- Use `if` to specify a block of code to be executed, if a specified condition is true.
- Use `else` to specify a block of code to be executed, if the same condition is false.
- Use `else if` to specify a new condition to test, if the first condition is false.
- Use `switch` to specify many alternative blocks of code to be executed.

### If...Else If...Else
```csharp
if (condition1)
{
    // block of code to be executed if condition1 is True
} 
else if (condition2) 
{
    // block of code to be executed if the condition1 is false and condition2 is True
} 
else
{
    // block of code to be executed if the condition1 is false and condition2 is False
}

### Short Hand If...Else (Ternary Operator)
There is also a short-hand `if else`, which is known as the ternary operator because it consists of three operands. It can be used to replace multiple lines of code with a single line. It is often used to replace simple `if else` statements.
```csharp
variable = (condition) ? expressionTrue : expressionFalse;
```

### Switch Statements
Use the `switch` statement to select one of many code blocks to be executed.
```csharp
switch(expression) 
{
    case x:
        // code block
        break;
    case y:
        // code block
        break;
    default:
        // code block
        break;
}
```
**This is how it works:**
- The `switch` expression is evaluated once.
- The value of the expression is compared with the values of each `case`.
- If there is a match, the associated block of code is executed.
- The `break` and `default` keywords will be described later in this chapter.

---

## 5. Loops (for, foreach, while)

**1. For loop:** When the number of iterations is known in advance.
```csharp
for (int i = 0; i < 5; i++) 
{ 
    /* code */ 
}
```

**2. While loop:** Loop while the condition is true.
```csharp
int count = 0;
while (count < 5) 
{ 
    count++; 
}
```

**3. Foreach loop:** Loop through the elements of an array/list.
```csharp
string[] fruits = { "Apple", "Banana", "Cherry" };

foreach (string fruit in fruits)
{ Console.WriteLine(fruit); }
```



## 6. Methods, ref / out in C#

### 1. Methods Overview
**Methods** are functions in C# used to perform a specific function.

**They help:**
- Reuse code.
- Organize the program more clearly.

**Example:**
```csharp
int Add(int a, int b)
{
    return a + b;
}
```

---

### 2. Passing by value (Default)
C# defaults to passing by value → creating a copy.
Changes within the function **do not affect** the outside variable.

**Example:**
```csharp
void ChangeValue(int x)
{
    x = 100;
}

int a = 10;
ChangeValue(a);

Console.WriteLine(a); // Output: 10 (remains 10)
```

---

### 3. The `ref` keyword
👉 **Pass by reference** (directly points to the original variable).

**Characteristics:**
- Must be initialized beforehand.
- Can be read and written within the function.

**Example:**
```csharp
void ChangeValue(ref int x)
{
    x = 100;
}

int a = 10;
ChangeValue(ref a);

Console.WriteLine(a); // Output: 100
```

---

### 4. The `out` keyword
👉 Also passes by reference, but primarily used to **return data**.

**Characteristics:**
- No prior initialization required.
- It is **MANDATORY** to assign a value within the function.

**Example:**
```csharp
void GetValues(out int x)
{
    x = 50;
}

int a; // No initialization needed
GetValues(out a);

Console.WriteLine(a); // Output: 50
```

---

### 5. Quick comparison of `ref` vs `out`

| Criteria | `ref` | `out` |
| :--- | :---: | :---: |
| **Requires prior initialization** | ✅ Yes | ❌ No |
| **Requires assignment within the function** | ❌ No | ✅ Yes |
| **Used when** | Want to modify a variable | Want to return a result |

#### When should you use it?
- **`ref`** → when you want to modify the original variable.
- **`out`** → when you need to return multiple values ​​from a function.
- **Otherwise** → avoid overuse, as it can easily make the code difficult to read.

## 7. Type Conversion

In .NET (including C#), **Type Conversion** is the process of changing a value from one data type to another. .NET provides various conversion mechanisms depending on the data safety level and the data types involved in the conversion.

Below are common types of data conversion in .NET.

---

### 1. Implicit Conversion
Implicit conversion occurs automatically by the compiler when the conversion is guaranteed to be **safe**, without data loss and without causing memory overflow errors.

* **For Value Types:** Converts from a smaller size or lower precision type to a larger size or higher precision type.

```csharp

int num = 123;

double bigNum = num; // Automatically convert from int to double (safe)

```
* **For Reference Types:** Converts from a derived class to the base class or interface it implements.

```csharp
string text = "Hello";

object obj = text; // Automatically converts from string to object

```

---
### 2. Explicit Conversion / Casting
Explicit conversion is used when the conversion risks **data loss** (e.g., converting from a real number to an integer, or converting from a larger to a smaller data type). The compiler requires you to use the cast operator `(data_type)` to confirm that you understand this risk.

* **For Value Types:**

```csharp
double pi = 3.14159;

int integerPi = (int)pi; // Manual type casting is required. The value will now only be 3 (losing the decimal part)

```
* **For reference types (Downcasting):** Converts from parent class to child class. This may cause an `InvalidCastException` error at runtime if the actual object is not of that child type.

```csharp

object obj = "This is a string";

string str = (string)obj; // Valid because obj actually contains a string

```

---
### 3. Conversion using Helper Classes
When converting between data types that are not directly compatible (such as from a string to an integer), you cannot use the regular cast operator. Instead, .NET provides helper methods:

#### a. The `System.Convert` class
Provides a series of static methods (`ToInt32`, `ToDouble`, `ToBoolean`,...) to convert between basic types. This class handles `null` values ​​quite well (returning the default value of the target type instead of throwing an exception).

```csharp
string input = "456";

int result = Convert.ToInt32(input); // Converts string to number 456
```

#### b. The `Parse` and `TryParse` methods
Often used to parse a string into the target data type.

* **`Parse`**: Throws an exception if the string is not correctly formatted.

* **`TryParse`**: Returns `true` or `false` indicating a successful or failed conversion, and does not throw an exception, resulting in better performance when handling uncertain data.

```csharp
string successString = "123";

string failString = "abc";

// Use Parse (Risk of throwing a FormatException)
int val1 = int.Parse(successString);

// Use TryParse (Recommended for safety)
if (int.TryParse(failString, out int val2))
{
Console.WriteLine($"Success: {val2}");

}
else
{
Console.WriteLine("Conversion failed.");

}
```

---
### 4. Safe Reference Type Conversion (`is` and `as`)
To avoid `InvalidCastException` errors when casting object classes, .NET supports two very useful operators:

* **The `as` operator**: Performs data type conversion. If the conversion fails, it returns `null` instead of throwing an error.

```csharp
object obj = 123; // This is actually an integer

string str = obj as string; // Conversion failed because obj is not a string, str receives a null value

```
* **The `is` operator combined with Pattern Matching**: Checks whether the object belongs to the target type and assigns the value to a new variable if it is.

```csharp
object obj = "Hello World";

if (obj is string message)

{
// If obj is a string, the variable message will be created and used in this block of code
Console.WriteLine(message.ToUpper());
}

```

---
### 5. User-Defined Conversions
You can define how your class or struct converts between different data types using the keywords `implicit` or `explicit`.

```csharp
public class Digit
{
public byte Value { get; }

public Digit(byte value)

{
Value = value;

}

// Defines implicit conversion from Digit to byte
public static implicit operator byte(Digit d) => d.Value;

// Defines explicit conversion from byte to Digit
public static explicit operator Digit(byte b) => new Digit(b); }

// Usage:
Digit d = new Digit(7);

byte number = d; // Default: No casting required

byte b = 9;

Digit d2 = (Digit)b; // Explicit: Required
```

# Reference: https://www.w3schools.com/cs , https://learn.microsoft.com/vi-vn/dotnet/csharp/language-reference/builtin-types/value-types
