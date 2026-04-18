using StudentManagementConsole.Models;
using StudentManagementConsole.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var student = new Student
        {
            Id = 1,
            Name = "Test",
            Grade = StudentGrade.A
        };

        var service = new StudentService();
        await service.Save(student);
        Console.WriteLine("End program");
    }
}