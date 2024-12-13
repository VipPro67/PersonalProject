namespace CourseApi.Helpers;
public class CourseQuery
{
    public string? CourseId { get; set; } = null;
    public string? CourseName { get; set;} = null;
    public string? Instructor { get; set; } = null;
    public string? Department { get; set; } = null;
    
    public int? CreditMin { get; set; } = null;
    public int? CreditMax { get; set; } = null;
    // public DateOnly? StartDateMin { get; set; } = null;
    // public DateOnly? StartDateMax { get; set; } = null;
    // public DateOnly? EndDateMin { get; set; } = null;
    // public DateOnly? EndDateMax { get; set; } = null;
    public string? Schedule { get; set; } = null;

    public int? Page { get; set; } = 1;
    public int? ItemsPerPage { get; set; } = 10;
}