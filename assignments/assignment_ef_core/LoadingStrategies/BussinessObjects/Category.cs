using System;
using System.Collections.Generic;
using System.Text;

namespace BussinessObject
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Note: virtual is compulsory for using Lazy Loading
        public virtual ICollection<Product> Products { get; set; }
    }
}
