using StudentApi.DTOs;
using FluentValidation;

namespace StudentApi.Validators;
public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not in a valid format")
            .MaximumLength(100).WithMessage("Email should not exceed 100 characters");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required")
            .MaximumLength(100).WithMessage("FullName should not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required")
            .MaximumLength(20).WithMessage("PhoneNumber should not exceed 20 characters")
            .Matches("[0-9]").WithMessage("PhoneNumber should only contain numeric characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("DateOfBirth is required")
            .LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("DateOfBirth should be a date in the past");

        RuleFor(x => x.Grade)
            .NotEmpty().WithMessage("Grade is required")
            .InclusiveBetween(1, 8).WithMessage("Grade should be between 1 and 8");

        RuleFor(x => x.Address)
            .MaximumLength(100).WithMessage("Address should not exceed 100 characters");
    }
}

