namespace AuthApi.Models;
public class AppUser
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
