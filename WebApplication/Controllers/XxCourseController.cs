using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApplication.Data;
using TalentBay1.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace TalentBay1.Controllers
{
    public class XxCourseController : Controller
    {

        private readonly ApplicationDbContext _dbContext;

        public XxCourseController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult ListCourses()
        {
            var instructorId = GetLoggedInInstructorId();
            var courses = _dbContext.Courses.Where(c => c.InstructorID == instructorId).ToList();

            return View(courses);
        }
        public IActionResult Edit(int id)
        {
            var course = _dbContext.Courses.Find(id);


            return View(course);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;

                // directory of images
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                // create images if it doesnt exist
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // imageUrl = filename
                course.ImageURL = uniqueFileName;
            }
            else
            {
                // default img url 
                course.ImageURL = "default-image.jpg";
            }
            course.InstructorID = GetLoggedInInstructorId();
            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();

                // set a sucess message
            ViewBag.SuccessMessage = "Course Created Successfully";

                // clear form fields
            ModelState.Clear();

           

            return View();
        }
        [HttpPost]
        public IActionResult Update(Course course)
        {
            _dbContext.Courses.Update(course);
            _dbContext.SaveChanges();

            ViewBag.SuccessMessage = "Course updated successfully";
            return View();
        }
            
        private string GetLoggedInInstructorId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }

    }
}
