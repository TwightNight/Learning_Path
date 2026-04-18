using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagementConsole.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public StudentGrade Grade { get; set; }
    }

    public enum StudentGrade
    {
        A, B, C, D, E
    }
}
