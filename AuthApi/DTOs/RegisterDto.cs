using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string? Password { get; set; }

        public string? FullName { get; set; }
        public string? Address { get; set; }
    }
}