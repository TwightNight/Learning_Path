using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    // [Required] : field must be input, cannot null or empty
    // [StringLength]: String length limit.(ex: Maximum 50 characters [StringLength(50)]
    // [StringLength((int)..., ErrorMessage = "...")] : String length limit with custom error message
    // [Display(Name = "...")]: change the name display in UI
    // [Column("...")] : map the name of colunm in DB (if not use this the default name is the name of property)
    // [DataType(...)] : tell with UI data type of field
    // [DisplayFormat(...)]: format showing data
    public class Student
    {
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Cannot be longer than 50 characters in First Name")]
        [Column("FirstName")]
        [Display(Name = "First Name")]
        public string FirstMidName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode =  true)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + " " + FirstMidName;
            }
        }
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}