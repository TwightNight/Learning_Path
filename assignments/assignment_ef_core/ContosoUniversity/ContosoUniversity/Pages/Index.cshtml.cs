using ContosoUniversity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SchoolContext _context;

        public IndexModel(SchoolContext context)
        {
            _context = context;
        }

        
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalDepartments { get; set; }

        public async Task OnGetAsync()
        {
            
            TotalStudents = await _context.Students.CountAsync();
            TotalCourses = await _context.Courses.CountAsync();
            TotalInstructors = await _context.Instructors.CountAsync();
            TotalDepartments = await _context.Departments.CountAsync();
        }
    }
}