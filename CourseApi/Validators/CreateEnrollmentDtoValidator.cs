using CourseApi.DTOs;
using FluentValidation;

namespace CourseApi.Validators;
public class CreatEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
{
    public CreatEnrollmentDtoValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required")
            .MaximumLength(10).WithMessage("CourseId should not exceed 10 characters")
            .MinimumLength(3).WithMessage("CourseId should not be less than 3 characters")
            .Matches(@"^[^\s]*$").WithMessage("CourseId should not contain any spaces")
            .Matches(@"^[a-zA-Z0-9]*$").WithMessage("CourseId should only contain alphanumeric characters");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("CourseName is required");
    }
}

