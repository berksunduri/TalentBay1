using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyApplication.Data;
using TalentBay1.Models;

namespace TalentBay1.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Assignments
        public async Task<IActionResult> Index()
        {
            SetLoggedInInstructorIdInViewBag();
            var applicationDbContext = _context.Assignment.Include(a => a.Course);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Assignment == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.AssignmentID == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // GET: Assignments/Create
        public IActionResult Create()
        {
            // Get the logged-in instructor's ID
            string loggedInInstructorId = GetLoggedInInstructorId();

            // Get the course IDs and titles for the current instructor
            var coursesForCurrentUser = _context.Courses
                .Where(course => course.InstructorID == loggedInInstructorId)
                .Select(course => new SelectListItem { Value = course.CourseID.ToString(), Text = course.Title })
                .ToList();

            // Create a SelectList for the dropdown
            SelectList courseSelectList = new SelectList(coursesForCurrentUser, "Value", "Text");

            // Set the ViewBag for the dropdown
            ViewBag.CourseID = courseSelectList;

            return View();
        }


        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentID,CourseID,Title,Description,DueDate")] Assignment assignment)
        {
            // Retrieve the associated course including assignments
            var course = _context.Courses.Include(c => c.Assignments).FirstOrDefault(c => c.CourseID == assignment.CourseID);

            // Ensure that the CourseID is valid and the course is found
            if (course == null || !_context.Courses.Any(c => c.CourseID == assignment.CourseID))
            {
                ModelState.AddModelError("CourseID", "Invalid CourseID");
            }
            else
            {
                // Set the Course property
                assignment.Course = course;
            }

            // Manually validate the Course property
            if (assignment.Course == null)
            {
                ModelState.AddModelError("Course", "Invalid Course");
            }

            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "CourseID", assignment.CourseID);
            return View(assignment);
        }




        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Assignment == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            // Get the course IDs and titles for the current instructor
            string loggedInInstructorId = GetLoggedInInstructorId();
            var coursesForCurrentUser = _context.Courses
                .Where(course => course.InstructorID == loggedInInstructorId)
                .Select(course => new SelectListItem { Value = course.CourseID.ToString(), Text = course.Title })
                .ToList();

            // Create a SelectList for the dropdown
            SelectList courseSelectList = new SelectList(coursesForCurrentUser, "Value", "Text", assignment.CourseID);

            // Set the ViewBag for the dropdown
            ViewBag.CourseID = courseSelectList;

            return View(assignment);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentID,CourseID,Title,Description,DueDate")] Assignment assignment)
        {
            if (id != assignment.AssignmentID)
            {
                return NotFound();
            }

            ModelState.Remove("Course");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.AssignmentID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "CourseID", assignment.CourseID);
            return View(assignment);
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Assignment == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.AssignmentID == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Assignment == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Assignment'  is null.");
            }
            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignment.Remove(assignment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssignmentExists(int id)
        {
          return (_context.Assignment?.Any(e => e.AssignmentID == id)).GetValueOrDefault();
        }

        private void SetLoggedInInstructorIdInViewBag()
        {
            ViewBag.LoggedInInstructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private string GetLoggedInInstructorId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
        private List<int> GetCourseIDsForCurrentUser(string instructorId)
        {
            // Assuming you have a DbSet<Course> named _context.Courses in your ApplicationDbContext
            var courseIds = _context.Courses
                .Where(course => course.InstructorID == instructorId)
                .Select(course => course.CourseID)
                .ToList();

            return courseIds;
        }
    }
}
