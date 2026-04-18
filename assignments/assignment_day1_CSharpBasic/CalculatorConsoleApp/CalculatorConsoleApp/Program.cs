// See https://aka.ms/new-console-template for more information
bool running = true;
 while (running)
{
    Console.Clear();
    Console.Write("Please enter a: ");
    double a = Convert.ToDouble(Console.ReadLine());
    Console.Write("Please enter operation(+, -, *, /, %): ");
    string op = Console.ReadLine();
    Console.Write("Please enter b: ");
    double b = Convert.ToDouble(Console.ReadLine());

    if (Calculate(a, b, op, out double result))
    {
        Console.WriteLine($"Result: {a} {op} {b} = {result}");
    }

    Console.WriteLine("Wanna countinue? (Y/N)");
    string isCountinue = Console.ReadLine();
    if (isCountinue.ToUpper() != "Y")
    {
        running = false;
    }
}

bool Calculate(double a, double b, string operation, out double result)
{
    result = 0;

    switch (operation)
    {
        case "+":
            result = a + b;
            break;
        case "-":
            result = a - b;
            break;
        case "*":
            result = a * b;
            break;
        case "/":
            result = a / b;
            break;
        case "%":
            result = a % b;
            break;
        default:
            Console.WriteLine("The operation is invalid");
            return false;
    };

    if (operation == "/" && b == 0)
    {
        Console.WriteLine("Error: Cannot divide by zero!");
        return false;
    }
    return true;
}