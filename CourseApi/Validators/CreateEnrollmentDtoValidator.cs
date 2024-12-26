using CourseApi.DTOs;
using FluentValidation;

namespace CourseApi.Validators;
public class CreatEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
{
    public CreatEnrollmentDtoValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required")
            .MaximumLength(10).WithMessage("Course ID should not exceed 10 characters")
            .MinimumLength(3).WithMessage("Course ID should not be less than 3 characters")
            .Matches(@"^[^\s]*$").WithMessage("Course ID should not contain any spaces")
            .Matches(@"^[a-zA-Z0-9]*$").WithMessage("Course ID should only contain alphanumeric characters");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Course Name is required");
    }
}

