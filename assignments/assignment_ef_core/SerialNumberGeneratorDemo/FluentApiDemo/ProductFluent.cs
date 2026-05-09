using System;
using System.Collections.Generic;
using System.Text;

namespace FluentApiDemo
{
    public class ProductFluent
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? SerialNumber { get; set; }
    }
}
