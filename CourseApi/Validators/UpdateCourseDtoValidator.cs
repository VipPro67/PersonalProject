using CourseApi.DTOs;
using FluentValidation;

namespace CourseApi.Validators;
public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseDtoValidator()
    {
        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("Course Name is required")
            .MaximumLength(100).WithMessage("Course Name should not exceed 100 characters")
            .MinimumLength(5).WithMessage("Course Name should not be less than 5 characters")
            .Matches("^[A-Za-z ]+$").WithMessage("Course Name should only contain alphabetic characters and spaces");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Course Description is required")
            .MaximumLength(500).WithMessage("Course Description should not exceed 500 characters");

        RuleFor(x => x.Credit)
            .NotEmpty().WithMessage("Credit is required")
            .GreaterThan(0).WithMessage("Credit should be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Credit should be less than or equal to 10");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(50).WithMessage("Department should not exceed 50 characters")
            .Matches("^[A-Za-z ]+$").WithMessage("Department should only contain alphabetic characters");

        RuleFor(x => x.Instructor)
            .NotEmpty().WithMessage("Instructor is required")
            .MaximumLength(100).WithMessage("Instructor should not exceed 100 characters")
            .Matches("^[A-Za-z ]+$").WithMessage("Instructor should only contain alphabetic characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start Date is required")
            .LessThan(x => x.EndDate).WithMessage("Start Date should be before End Date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End Date is required");

        RuleFor(x => x.Schedule)
        .NotEmpty().WithMessage("Schedule is required")
        .MaximumLength(100).WithMessage("Schedule should not exceed 100 characters");
    }


}

