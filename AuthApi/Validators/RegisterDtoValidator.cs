using FluentValidation;
using AuthApi.DTOs;

namespace AuthApi.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username must be at most 50 characters long")
            .Matches("[a-zA-Z0-9]").WithMessage("Username must contain only alphanumeric characters");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(50).WithMessage("Password must be at most 50 characters long")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required");
        RuleFor(x => x.FullName).MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");
        RuleFor(x => x.Address).MaximumLength(200).WithMessage("Address cannot exceed 200 characters");
    }
}
