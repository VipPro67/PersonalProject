namespace CourseApi.DTOs;

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public string? CourseId { get; set; }
    public int? StudentId { get; set; }

    public virtual CourseDto Course { get; set; }
    public virtual StudentDto Student { get; set; }
}