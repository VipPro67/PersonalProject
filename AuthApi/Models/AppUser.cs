using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Models;

[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class AppUser
{
    [Key] // Primary key
    public int UserId { get; set; }
    [Required] // not null
    [StringLength(50, MinimumLength = 5)] // varchar(50)
    public string? UserName { get; set; }
    [Required]
    public string? PasswordHash { get; set; }
    [Required]
    [MaxLength(100)] // varchar(100)
    [EmailAddress]
    public string? Email { get; set; }
    //make this type nvarchar(100)
    [MaxLength(100)]
    public string? FullName { get; set; }
    [MaxLength(255)]
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
