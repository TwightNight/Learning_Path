using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Authorization;

namespace ContosoUniversity.Pages.Students
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ContosoUniversity.Data.SchoolContext _context;

        public IndexModel(ContosoUniversity.Data.SchoolContext context)
        {
            _context = context;
        }
        //sort parameters through URL
        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        //public string CurrentSort { get; set; }

        //filter date
        public DateTime? CurrentFromDate { get; set; }
        public DateTime? CurrentToDate { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public IList<Student> Students { get;set; } = default!;

        public async Task OnGetAsync(
            string sortOrder, 
            string searchString,
            DateTime? fromDate,
            DateTime? toDate,
            int pageIndex = 1)
        {
            // number of records per page
            int pageSize = 5;

            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            CurrentFilter = searchString;
            CurrentFromDate = fromDate;
            CurrentToDate = toDate;

            //The method uses LINQ to Entities to specify the column to sort by.
            //The code initializes an IQueryable<Student> before the switch statement, and modifies it in the switch statement
            //When an IQueryable is created or modified, no query is sent to the database. The query isn't executed until the IQueryable object is converted into a collection.
            //IQueryable are converted to a collection by calling a method such as ToListAsync
            IQueryable<Student> studentsIQ = from s in _context.Students
                                             select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                studentsIQ = studentsIQ.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString)
                                       || (s.LastName + " " + s.FirstMidName).Contains(searchString));
            }
            if (fromDate.HasValue)
            {
                studentsIQ = studentsIQ.Where(s => s.EnrollmentDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                studentsIQ = studentsIQ.Where(s => s.EnrollmentDate <= toDate.Value);
            }

            switch (sortOrder)
            {
                // sort following student name
                case "name_desc":
                    studentsIQ = studentsIQ.OrderByDescending(s => s.LastName);
                    break;
                // sort following Enrollment Date (asc/desc)
                case "Date":
                    studentsIQ = studentsIQ.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    studentsIQ = studentsIQ.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    studentsIQ = studentsIQ.OrderBy(s => s.LastName);
                    break;
            }

            var count = await studentsIQ.CountAsync();

            // calculate total pages 
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // save the current page 
            CurrentPage = pageIndex;

            //IQueryable code results in a single query that's not executed until the following statement:
            // paging: 
            // skip skips the previous page records 
            // take Gets records in the current page 
            Students = await studentsIQ
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
