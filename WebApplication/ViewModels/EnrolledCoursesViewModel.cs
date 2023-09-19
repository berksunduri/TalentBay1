using TalentBay1.Models;

namespace TalentBay1.ViewModels
{
    public class EnrolledCoursesViewModel
    {
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
    }

}

