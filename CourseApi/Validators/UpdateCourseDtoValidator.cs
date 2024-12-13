using CourseApi.DTOs;
using FluentValidation;

namespace CourseApi.Validators;
public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseDtoValidator()
    {
        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("CourseName is required")
            .MaximumLength(100).WithMessage("CourseName should not exceed 100 characters")
            .MinimumLength(5).WithMessage("CourseName should not be less than 5 characters")
            .Matches("^[A-Za-z ]+$").WithMessage("CourseName should only contain alphabetic characters and spaces");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("CourseDescription is required")
            .MaximumLength(500).WithMessage("CourseDescription should not exceed 500 characters");

        RuleFor(x => x.Credit)
            .NotEmpty().WithMessage("Credit is required")
            .GreaterThan(0).WithMessage("Credit should be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Credit should be less than or equal to 10");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(50).WithMessage("DepartmentName should not exceed 50 characters")
            .Matches("^[A-Za-z ]+$").WithMessage("DepartmentName should only contain alphabetic characters");

        RuleFor(x => x.Instructor)
            .NotEmpty().WithMessage("InstructorName is required")
            .MaximumLength(100).WithMessage("InstructorName should not exceed 100 characters")
            .Matches("^[A-Za-z ]+$").WithMessage("InstructorName should only contain alphabetic characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required")
            .LessThan(x => x.EndDate).WithMessage("StartDate should be before EndDate");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required");

        RuleFor(x => x.Schedule)
        .NotEmpty().WithMessage("Schedule is required")
        .MaximumLength(100).WithMessage("Schedule should not exceed 100 characters");
    }


}

