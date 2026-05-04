using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFGetStarted
{
    //A DbContext instance represents a session with the database and can be used to query and save instances of your entities.
    //DbContext is a combination of the Unit Of Work and Repository patterns.
    //Without DbContext, C# wouldn't know how to communicate with the database.
    //It helps translate C# statements into SQL statements that the database understands.
    //Change Tracking: When you retrieve an object from the database and modify it, the DbContext tracks that change.
    //When you call the SaveChanges() function, it automatically generates the corresponding UPDATE or INSERT statement to save the changes to the database.
    public class BloggingContext : DbContext
    {
        //DbSet<T> represents a Table in the database, where T is an Entity representing a data row in that table.
        //It allows you to perform CRUD (Create, Read, Update, Delete) operations on that table through C# code (using LINQ) instead of having to write complex manual SQL statements.
        //Instead of writing SQL: SELECT * FROM Blogs, you can simply write C# code: var blogs = db.Blogs.ToList();
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public string DbPath { get; }

        public BloggingContext()
        {
            //var forder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(forder);
            //DbPath = System.IO.Path.Join(path, "blogging.db");
            DbPath = Path.Combine(Directory.GetCurrentDirectory(), "blogging.db");
        }
        //OnConfiguring is a built-in function of DbContext; you override it to configure this context.
        //DbContextOptionsBuilder is the object that helps you set the options.
        //EF Core supports many different database types (SQL Server, MySQL, PostgreSQL, Oracle, SQLite, etc.).
        //EF Core cannot automatically know which type you want to use or where your database is located.
        //This function is where you "declare" to EF Core, example: "I want to use SQLite, and my data file is located at DbPath."
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}
