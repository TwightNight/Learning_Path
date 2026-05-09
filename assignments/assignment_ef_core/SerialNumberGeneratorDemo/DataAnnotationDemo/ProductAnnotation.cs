using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAnnotationDemo
{
    public class ProductAnnotation
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // This indicates to EF that the database will handle this column itself and that SQL formulas cannot be passed to it.
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? SerialNumber { get; set; }
    }
}
