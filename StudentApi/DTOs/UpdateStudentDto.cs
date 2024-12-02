namespace StudentApi.DTOs;

public class UpdateStudentDto
{
    public int StudentId { get; set;}
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int Grade { get; set; }

}