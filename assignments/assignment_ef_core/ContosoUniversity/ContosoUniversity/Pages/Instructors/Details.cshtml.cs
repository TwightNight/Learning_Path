using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Pages.Instructors
{
    public class DetailsModel : PageModel
    {
        private readonly ContosoUniversity.Data.ISchoolContext _context;

        public DetailsModel(ContosoUniversity.Data.ISchoolContext context)
        {
            _context = context;
        }

        public Instructor Instructor { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors.FirstOrDefaultAsync(m => m.Id == id);

            if (instructor is not null)
            {
                Instructor = instructor;

                return Page();
            }

            return NotFound();
        }
    }
}
