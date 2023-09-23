using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // COURSE STUFF FOR ADMINS


        public async Task<IActionResult> CourseIndex()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }


        public IActionResult CourseCreate()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseCreate([Bind("CourseID,Title,Description,InstructorID,Category,EnrollmentCount,ImageURL")] Course course)
        {
            if (ModelState.IsValid)
            {
                // Save the course to the database
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CourseIndex));
            }
            return View(course);
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
                return RedirectToAction(nameof(CourseIndex));
            }
            return View(course);
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

            // Pass both course and enrollments to the view
            ViewData["Course"] = course;
            ViewData["Enrollments"] = enrollments;

            return View();
        }




        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }

    }
}
