using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Pages.Departments
{
    public class DeleteModel : PageModel
    {
        private readonly ContosoUniversity.Data.SchoolContext _context;

        public DeleteModel(ContosoUniversity.Data.SchoolContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Department Department { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d=>d.Administrator)
                .Include(d=>d.Courses)
                .FirstOrDefaultAsync(m => m.DepartmentID == id);

            if (department is not null)
            {
                Department = department;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

           
            var department = await _context.Departments
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(m => m.DepartmentID == id);

            if (department != null)
            {
               
                if (department.Courses.Any())
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete department because it has courses.");
                    Department = department;
                    return Page();
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
