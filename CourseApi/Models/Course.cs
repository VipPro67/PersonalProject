using System.ComponentModel.DataAnnotations;

namespace CourseApi.Models;
public class Course
{
    [Key]
    [Required]
    [MaxLength(10)]
    public string? CourseId { get; set; }

    [Required]
    [MaxLength(50)]
    public string? CourseName { get; set; }
    [Required]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int? Credit { get; set; }

    [Required]
    [MaxLength(50)]

    public string? Instructor { get; set; }

    [Required]
    [MaxLength(50)]
    public string? Department { get; set; }

    [Required]
    public DateOnly? StartDate { get; set; }

    [Required]
    public DateOnly? EndDate { get; set; }

    [Required]
    public string? Schedule { get; set; }
}
