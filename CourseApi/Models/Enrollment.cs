namespace CourseApi.Models;
public class Enrollment
{
    public int EnrollmentId { get; set; }
    public string? CourseId { get; set; }
    public int? StudentId { get; set; }

    public virtual Course Course { get; set; } = null!;
    public virtual Student Student { get; set; } = null!;
}

