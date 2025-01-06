namespace StudentApi.Helpers;
public class StudentQuery
{
    public string? StudentName { get; set; } = null;
    public string? Email { get; set; } = null;
    public string? PhoneNumber { get; set; } = null;

    //public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; } = null;
    public int? GradeMin { get; set; } = 1;
    public int? GradeMax { get; set; } = 8;

    public string? SortBy { get; set; } = "StudentId";
    public string? SortByDirection { get; set; } = "asc";
    public int? Page { get; set; } = 1;
    public int? ItemsPerPage { get; set; } = 10;

}