﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Data;
using System.Security.Claims;
using TalentBay1.Models;
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

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Courses == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }


        public IActionResult BrowseCourses(string searchTerm)
        {
            // Fetch unique categories from the database
            var categories = _context.Courses.Select(c => c.Category).Distinct().ToList();

            ViewBag.Categories = categories;  // Pass categories to the view

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

        public IActionResult Assignments(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Get the course and its assignments
            var course = _context.Courses.Include(c => c.Assignments).FirstOrDefault(c => c.CourseID == id);
            if (course == null)
                return NotFound();

            // Retrieve the assignments for this course
            var assignments = course.Assignments;

            // Retrieve completed assignments for this student
            var completedAssignmentIds = _context.StudentAssignments
                                                .Where(sa => sa.StudentId == userId && sa.IsCompleted)
                                                .Select(sa => sa.AssignmentId)
                                                .ToList();

            // Mark assignments as completed based on the completedAssignmentIds
            foreach (var assignment in assignments)
            {
                assignment.IsCompleted = completedAssignmentIds.Contains(assignment.AssignmentID);
            }

            return View(assignments);
        }


        public IActionResult MarkAssignmentComplete(int assignmentId)
        {
            var userId = _userManager.GetUserId(User);

            var assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignmentId);

            if (assignment == null)
            {
                return NotFound(); // Assignment not found
            }

            var courseId = assignment.CourseID;

            // Check if this assignment is already marked as completed for the user
            var existingRecord = _context.StudentAssignments
                .FirstOrDefault(sa => sa.AssignmentId == assignmentId && sa.StudentId == userId);

            if (existingRecord != null)
            {
                // Assignment is already marked as completed, update the completion status
                existingRecord.IsCompleted = true;
            }
            else
            {
                // Create a new StudentAssignment record
                var studentAssignment = new StudentAssignment
                {
                    StudentId = userId,
                    AssignmentId = assignmentId,
                    CourseId = courseId,
                    IsCompleted = true
                };

                _context.StudentAssignments.Add(studentAssignment);
            }

            _context.SaveChanges();

            return RedirectToAction("Assignments", new { id = courseId });
        }





        [HttpPost]
        public IActionResult UpdateAssignmentCompletion(int assignmentId, bool isCompleted)
        {
            try
            {
                // Find the assignment by assignmentId
                var assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignmentId);

                if (assignment == null)
                    return NotFound(); // Or any other appropriate error response

                // Update the completion status
                assignment.IsCompleted = isCompleted;

                // Save changes to the database
                _context.SaveChanges();

                return Ok(); // Return a success response
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log or return an error response)
                return StatusCode(500, "An error occurred while updating assignment completion.");
            }
        }

        [HttpGet]
        public IActionResult GetAssignmentCompletionStatus(int assignmentId)
        {
            var assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentID == assignmentId);

            if (assignment == null)
                return NotFound();

            return Json(new { isCompleted = assignment.IsCompleted });
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
    }
}