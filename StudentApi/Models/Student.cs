using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace StudentApi.Models;

[Index(nameof(Email), IsUnique = true)]
public class Student
{
    [Key]
    [Required]
    public int StudentId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? FullName { get; set; }

    [Required]
    [StringLength(50)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public DateOnly DateOfBirth { get; set; }

    [MaxLength(100)]
    public string? Address { get; set; }

    public int Grade { get; set; }

}
