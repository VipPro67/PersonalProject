namespace CourseApi;
public class CourseDto
{
    public string? CourseId { get; set; }

    public string? CourseName { get; set; }
    public string? Description { get; set; }

    public int? Credit { get; set; }

    public string? Instructor { get; set; }

    public string? Department { get; set; }

    public string? StartDate { get; set; }

    public string? EndDate { get; set;}
    public string? Schedule { get; set; }
}
