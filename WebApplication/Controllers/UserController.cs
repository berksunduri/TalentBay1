﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyApplication.Data;
using TalentBay1.Models;

namespace TalentBay1.Controllers
{
    public class UserController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: UserController
        public async Task<ActionResult> UserIndex()
        {
            var users = await RetrieveUsersAsync();
            return View(users);  // Pass the users to the view
        }
        private async Task<List<IdentityUser>> RetrieveUsersAsync()
        {
            // Retrieve all users
            var users = await _userManager.Users.ToListAsync();

            return users;
        }
        // GET: UserController/Details/5
        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Get the user's roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Pass the user and roles to the view
            ViewData["Roles"] = userRoles;
            return View(user);
        }

        // GET: UserController/Create
        public ActionResult UserCreate()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserCreate(IFormCollection form)
        {
            try
            {
                var email = form["Email"];
                var password = form["Password"];
                var role = form["Role"]; // This will contain the selected role (Admin, Instructor, Student)

                // Create a new IdentityUser
                var user = new IdentityUser { UserName = email, Email = email };

                // Save the user to the database
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Add the user to the specified role
                    await _userManager.AddToRoleAsync(user, role);

                    // Redirect to the UserIndex action
                    return RedirectToAction(nameof(UserIndex));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
                return View();
            }
        }


        public async Task<IActionResult> UserEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Get the user's current roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Pass the user and roles to the view
            ViewData["Roles"] = userRoles;
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(string id, [Bind("Id,Email,Password")] IdentityUser user, string selectedRoles)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the user
                    var existingUser = await _userManager.FindByIdAsync(id);
                    existingUser.Email = user.Email;

                    // Update other user properties as needed

                    // Remove current roles
                    var userRoles = await _userManager.GetRolesAsync(existingUser);
                    await _userManager.RemoveFromRolesAsync(existingUser, userRoles);

                    // Add selected roles
                    var rolesToAdd = selectedRoles.Split(',');
                    await _userManager.AddToRolesAsync(existingUser, rolesToAdd);

                    await _userManager.UpdateAsync(existingUser);

                    return RedirectToAction("UserIndex");
                }
                catch (Exception)
                {
                    // Handle errors appropriately
                    return View(user);
                }
            }

            // Model state is not valid, return to the edit view
            return View(user);
        }


        [HttpGet] // This is for displaying the confirmation page
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserDelete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost] // This is for handling the actual deletion
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDeleteConfirmed(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("UserIndex");
            }

            // Handle errors appropriately
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollUser(int courseId, string userId)
        {
            try
            {
                var course = await _context.Courses.FindAsync(courseId);

                if (course == null)
                {
                    return NotFound();
                }

                var enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseID = courseId,
                    EnrollmentDate = DateTime.Now
                };

                // Increment the enrollment count
                course.EnrollmentCount++;

                // Save changes to the database
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                return RedirectToAction("CourseDetails", new { id = courseId });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                ModelState.AddModelError(string.Empty, "An error occurred while enrolling the user.");
                return RedirectToAction("CourseDetails", new { id = courseId });
            }
        }

        // COURSE STUFF FOR ADMINS


        public async Task<IActionResult> CourseIndex()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }


        public IActionResult CourseCreate()
        {
            var instructors = _userManager.GetUsersInRoleAsync("Instructor").Result;
            ViewBag.Instructors = new SelectList(instructors, "Id", "Email");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseCreate([Bind("CourseID,Title,Description,InstructorID,Category,EnrollmentCount,ImageFile")] Course course)
        {
            if (course.ImageFile != null && course.ImageFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + course.ImageFile.FileName;
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await course.ImageFile.CopyToAsync(stream);
                }

                course.ImageURL = "/images/" + uniqueFileName; // Set the relative URL
            }
            else
            {
                course.ImageURL = "default-image.jpg";
            }

            // Manually clear the validation state for ImageURL
            ModelState.Remove("ImageURL");

            // Manually validate the ImageURL property
            if (string.IsNullOrEmpty(course.ImageURL))
            {
                ModelState.AddModelError("ImageURL", "Image URL is required.");
            }
               
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("CourseIndex", "User");
            }

            return RedirectToAction("CourseIndex", "User");
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return "/images/" + uniqueFileName; // Return the relative URL
            }
            return null; // Return null if file is null or empty
        }
        public async Task<IActionResult> CourseEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEdit(int id, [Bind("CourseID,Title,Description,InstructorID,Category,EnrollmentCount,ImageURL")] Course course)
        {
            if (id != course.CourseID)
            {
                return NotFound();
            }

            ModelState.Remove("ImageFile");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("CourseIndex", "User");
            }
            return RedirectToAction("CourseIndex", "User");
        }

        public async Task<IActionResult> CourseDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost, ActionName("CourseDelete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseDeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CourseIndex));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CourseDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            // Retrieve enrollments for this course
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Where(e => e.CourseID == id)
                .ToListAsync();

            // Get all registered users
            var allUsers = await _context.Users.ToListAsync();

            // Get the IDs of users already enrolled in the course
            var enrolledUserIds = enrollments.Select(e => e.UserId);

            // Filter out users who are already enrolled in the course
            var notEnrolledUsers = allUsers.Where(user => !enrolledUserIds.Contains(user.Id)).ToList();

            // Pass both course, enrollments, and notEnrolledUsers to the view
            ViewData["Course"] = course;
            ViewData["Enrollments"] = enrollments;
            ViewBag.NotEnrolledUsers = notEnrolledUsers;

            return View();
        }



        // GET: Assignments/Create
        public IActionResult AssignmentCreate()
        {
            // Get all courses
            var allCourses = _context.Courses.Select(course => new SelectListItem { Value = course.CourseID.ToString(), Text = course.Title }).ToList();

            // Create a SelectList for the dropdown
            SelectList courseSelectList = new SelectList(allCourses, "Value", "Text");

            // Set the ViewBag for the dropdown
            ViewBag.CourseID = courseSelectList;

            return View();
        }


        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignmentCreate([Bind("AssignmentID,CourseID,Title,Description,DueDate")] Assignment assignment)
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
            ModelState.Remove("StudentAssignments");

              if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction("CourseIndex", "User"); ;
            }

            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "CourseID", assignment.CourseID);
            return View(assignment);
        }





        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }

    }
}
