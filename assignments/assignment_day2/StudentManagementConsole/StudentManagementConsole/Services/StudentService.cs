using StudentManagementConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagementConsole.Services
{
    public class StudentService
    {
        public async Task Save(Student student)
        {
            Console.WriteLine($"Saving... {student.Name}");
            await Task.Delay(2000);
            Console.WriteLine("Saved");
        }
    }
}
