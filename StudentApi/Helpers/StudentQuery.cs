namespace StudentApi.Helpers;
public class StudentQuery
{
    public string? StudentName { get; set; } = null;
    public string? Email { get; set; } = null;
    public string? PhoneNumber { get; set; } = null;

    //public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int? GradeMin { get; set; }
    public int? GradeMax { get; set; }

    public int? Page { get; set; } = 1;
    public int? ItemsPerPage { get; set; } = 10;

}