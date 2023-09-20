using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using TalentBay1.Models;

public class Assignment
{
    public int AssignmentID { get; set; }
    public int CourseID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }

    public virtual Course Course { get; set; }

    public bool IsCompleted { get; set; }

    public ICollection<StudentAssignment> StudentAssignments { get; set; }
}
