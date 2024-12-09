namespace StudentApi.DTOs;

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public int? StudentId { get; set; }

    public string? StudentName { get; set; }

}