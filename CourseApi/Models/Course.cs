namespace CourseApi.Models;
public class Course
{
    public string? CourseId { get; set; }

    public string? CourseName { get; set; }
    public string? Description { get; set; }

    public int? Credit { get; set; }

    public string? Instructor { get; set; }
    public string? Department { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Schedule { get; set; }

    public List<Enrollment>? Enrollments { get; set; }
}
