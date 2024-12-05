namespace CourseApi.Models;
public class Student
{
    public int StudentId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int Grade { get; set; }

    public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
