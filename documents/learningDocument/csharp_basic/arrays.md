# 📚 OVERVIEW OF ARRAYS IN C#

## 1. What is an Array?
An **Array** in C# is a data structure used to store a collection (or sequence) of elements. These elements share the **same data type**, are stored in **contiguous memory locations**, and the array has a **fixed size** determined at the time of initialization.

Each element in an array is accessed via an **index**. 
- The first element always has an index of `0`.
- The last element has an index of `n - 1` (where `n` is the total number of elements in the array).

### 💡 Key Characteristics of Arrays:
- **Fixed Length:** You cannot increase or decrease the size of the array once it has been created.
- **Homogeneous:** It only stores elements of the same data type (e.g., entirely `int`, or entirely `string`).
- **Fast Access:** Because elements are stored in contiguous memory, accessing any element via its index is extremely fast ($O(1)$ time complexity).

### 1.1. Declaration and Initialization Syntax

There are three common ways to create a one-dimensional array in C#:

**Method 1: Declare the number of elements first (The initial default value will be 0 for numbers, an empty array for strings)**
```csharp

// Declare an array that can hold 5 integers
int[] numbers = new int[5];

// Assign the following values
numbers[0] = 10;

numbers[1] = 20;

// The elements numbers[2], numbers[3], numbers[4] currently have a value of 0
```

**Method 2: Declare and assign values ​​directly**
```csharp
string[] fruits = new string[] { "Apple", "Banana", "Cherry" };

```
**Method 3: Shortcut Syntax (Most concise, most commonly used)**
```csharp
double[] prices = { 15.5, 20.0, 9.99, 50.2 };

```

### 1.2. Two common ways to iterate through a one-dimensional array

To iterate through each element in a one-dimensional array, we usually use a `for` or `foreach` loop. The `.Length` property is used to get the total number of elements in the array.

```csharp
string[] cars = { "Volvo", "BMW", "Ford", "Mazda" };

// Method 1: Using a FOR loop (When you need to know the exact position/index of the element)
Console.WriteLine("--- Use a FOR loop ---");

for (int i = 0; i < cars.Length; i++)
{
Console.WriteLine($"The car at position {i} is: {cars[i]}");

}

// Method 2: Using a FOREACH loop (Short, easy to read when only needing to get the value)
Console.WriteLine("--- Use a FOREACH loop ---");

foreach (string car in cars)
{
Console.WriteLine(car);

}
```
### Classification:
Based on how data is structured, C# arrays are divided into 3 main categories:
1. **Single-dimensional Arrays:** Stores data in a single row.
2. **Multi-dimensional Arrays (Rectangular Arrays):** Most commonly 2D arrays (like matrices or spreadsheets).
3. **Jagged Arrays:** An "array of arrays" (where each row can have a different number of columns).

---

## 2. Two-Dimensional Arrays (2D Arrays)

Unlike single-dimensional arrays that store data in a single line, **2D arrays** allow us to store data in a grid format consisting of **rows** and **columns**. You can think of it as a Microsoft Excel spreadsheet or a mathematical matrix.

### 2.1. Declaration and Initialization

In C#, a 2D rectangular array (where every row has the exact same number of columns) is declared using a comma `,` inside square brackets `[,]`.

**Method 1: Declare the size first, assign values later**
```csharp
// Declare a 2D integer array with 3 rows and 4 columns
int[,] matrix = new int[3, 4];

// Assign a value to the element at row 0, column 1
matrix[0, 1] = 10; 
```

**Method 2: Declare and initialize simultaneously**
```csharp
// Declare an array with 2 rows and 3 columns
int[,] numbers = new int[,] 
{
    { 1, 2, 3 }, // Row 0 (Index 0)
    { 4, 5, 6 }  // Row 1 (Index 1)
};

// Shorthand syntax (omitting 'new int[,]')
string[,] ticTacToe = 
{
    { "X", "O", "X" },
    { "O", "X", "O" },
    { "X", " ", " " }
};
```

### 2.2. Accessing and Modifying Elements
You use the index `[row, column]` to retrieve or modify a value.
```csharp
int[,] grid = {
    { 10, 20 },
    { 30, 40 }
};

Console.WriteLine(grid[1, 0]); // Outputs 30 (Row 1, Column 0)

grid[0, 1] = 99; // Changes the number 20 to 99
```

### 2.3. Iterating through a 2D Array
To loop through all elements in a 2D array, we typically use **two nested `for` loops**. 
In C#, the `GetLength(0)` method returns the number of rows, and `GetLength(1)` returns the number of columns.

#### 🎯 Concrete Example: Matrix Input and Output Program
Below is a complete C# program demonstrating how to create, assign, and print a 2D array to the console:

```csharp
using System;

class Program
{
    static void Main()
    {
        // Initialize a 2D array (3 rows, 4 columns)
        int[,] myMatrix = {
            { 1, 2, 3, 4 },
            { 5, 6, 7, 8 },
            { 9, 10, 11, 12 }
        };

        // Get the number of rows and columns
        int rows = myMatrix.GetLength(0);    // Returns 3
        int columns = myMatrix.GetLength(1); // Returns 4

        Console.WriteLine($"The matrix has {rows} rows and {columns} columns:");
        Console.WriteLine("-------------------------------------------");

        // Loop through each row
        for (int i = 0; i < rows; i++)
        {
            // Loop through each column in the current row
            for (int j = 0; j < columns; j++)
            {
                // Print the element with a tab space for alignment
                Console.Write(myMatrix[i, j] + "\t"); 
            }
            // Move to the next line after printing a complete row
            Console.WriteLine(); 
        }
    }
}
```
**Console Output:**
```text
The matrix has 3 rows and 4 columns:
-------------------------------------------
1       2       3       4	
5       6       7       8	
9       10      11      12	
```

---

## 3. Bonus: Rectangular Arrays vs. Jagged Arrays

When dealing with arrays that have multiple rows in C#, it is highly important to distinguish between two concepts:

1. **Multi-dimensional (Rectangular) Array - `[,]`**:
   - Every row has an **equal number of columns** (forms a perfect rectangle).
   - Syntax: `int[,]`
   - Best used for math matrices, game boards (like chess), etc.

2. **Jagged Array - `[][]`**:
   - Essentially an **"Array of arrays"**.
   - Each row can have a **different number of columns** (making the right edge look "jagged").
   - Best used to save memory when your data structure has uneven rows.

**Example of a Jagged Array:**
```csharp
// Declare an array with 3 rows (column sizes are not yet defined)
int[][] jaggedArray = new int[3][];

// Initialize different column sizes for each row
jaggedArray[0] = new int[] { 1, 2 };       // Row 0 has 2 columns
jaggedArray[1] = new int[] { 3, 4, 5, 6 }; // Row 1 has 4 columns
jaggedArray[2] = new int[] { 7 };          // Row 2 has only 1 column

// Accessing an element:
Console.WriteLine(jaggedArray[1][2]); // Outputs 5 (Row 1, Index 2 inside that row)
```
---
### Reference: https://www.youtube.com/watch?v=lt2nVeut6JI&list=PLRhlTlpDUWsxbj9D_2ZE1QzxRT0cuvo6N&index=22