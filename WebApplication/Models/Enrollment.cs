using System;
using System.ComponentModel.DataAnnotations;

namespace TalentBay1.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int CourseID { get; set; }

        [Required]
        public DateTime EnrollmentDate { get; set; }

        public virtual Course Course { get; set; }
    }
}
