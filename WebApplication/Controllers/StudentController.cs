using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyApplication.Data;
using System.Security.Claims;
using TalentBay1.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TalentBay1.ViewModels;

namespace TalentBay1.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BrowseCourses(string searchTerm)
        {
            var courses = _context.Courses.ToList();

            // Filter courses based on the search term (case-insensitive)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                courses = courses.Where(c => c.Title.ToLower().Contains(searchTerm)).ToList();
            }

            var enrolledCourses = GetEnrolledCourses(); // Get the enrolled course IDs for the current user
            ViewBag.EnrolledCourses = enrolledCourses;

            return View(courses);
        }

        public IActionResult EnrolledCourses()
        {
            var userId = _userManager.GetUserId(User);
            var enrolledCourses = _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course) // Include the Course information
                .ToList();

            var viewModel = new EnrolledCoursesViewModel
            {
                Courses = enrolledCourses.Select(e => e.Course),
                Enrollments = enrolledCourses
            };

            return View(viewModel);
        }

        public IActionResult Enroll(int courseId)
        {
            // Find the course
            var course = _context.Courses.FirstOrDefault(c => c.CourseID == courseId);

            if (course == null)
            {
                return NotFound();
            }

            // Create a new enrollment
            var enrollment = new Enrollment
            {
                UserId = GetLoggedInStudentId(), // Replace with the actual user ID
                CourseID = courseId,
                EnrollmentDate = DateTime.Now
            };

            // Increment the enrollment count
            course.EnrollmentCount++;

            // Save changes to the database
            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            return RedirectToAction("BrowseCourses");
        }

        private string GetLoggedInStudentId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }



        private List<int> GetEnrolledCourses()
        {
            var userId = _userManager.GetUserId(User);
            return _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseID)
                .ToList();
        }

        public IActionResult SearchCourses(string searchTerm)
        {
            var courses = _context.Courses.ToList();

            // Filter courses based on the search term (case-insensitive)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                courses = courses.Where(c => c.Title.ToLower().Contains(searchTerm)).ToList();
            }

            return PartialView("_CourseResults", courses);
        }


        //Enroll => Enrollment table CourseID UserID eslestir ve Course table-Enrollment +1 yap
    }
}