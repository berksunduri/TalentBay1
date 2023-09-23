using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class StudentAssignment
{
    [Key]
    public int Id { get; set; }

    // Foreign key for Student (ASP.NET Core Identity user ID)
    public string StudentId { get; set; }

    // Foreign key for Assignment
    public int AssignmentId { get; set; }

    public int CourseId { get; set; }

    // Completion status for this student
    public bool IsCompleted { get; set; }

    // Navigation properties
    public IdentityUser Student { get; set; } // ApplicationUser represents the Student
    public Assignment Assignment { get; set; }
}



