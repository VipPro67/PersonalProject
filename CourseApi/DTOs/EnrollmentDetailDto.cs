namespace CourseApi.DTOs;

public class EnrollmentDetailDto
{
    public int EnrollmentId { get; set; }
    public string? CourseId { get; set; }
    public CourseDto Course { get; set; } = new CourseDto();
    public int? StudentId { get; set; }

    public StudentDto Student { get; set; } = new StudentDto();

}