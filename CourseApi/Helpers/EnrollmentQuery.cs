namespace CourseApi.Helpers;
public class EnrollmentQuery
{
    public string? CourseId { get; set; } = null;
    
    public int? StudentId { get; set; } = null;

    public int? Page { get; set; } = 1;
    public int? ItemsPerPage { get; set; } = 10;
}