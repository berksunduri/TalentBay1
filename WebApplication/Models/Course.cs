using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TalentBay1.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string InstructorID { get; set; } // Foreign key referencing Users
        public string Category { get; set; }
        public int EnrollmentCount { get; set; }
        public string ImageURL { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }
    }
}
