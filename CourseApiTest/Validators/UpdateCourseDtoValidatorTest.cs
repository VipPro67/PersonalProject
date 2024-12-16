using FluentValidation.TestHelper;
using CourseApi.DTOs;
using CourseApi.Validators;
namespace CourseApiTest.Validators;

public class UpdateCourseApiValidatorTest
{
    public readonly UpdateCourseDtoValidator _validator;
    public UpdateCourseApiValidatorTest()
    {
        _validator = new UpdateCourseDtoValidator();
    }

    [Fact]
    public void UpdateCourseDtoValidator_ValidInput_PassValidation()
    {
        // Arrange
        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = "Course Test",
            Description = "This is a course create for testing",
            Credit = 4,
            Department = "Test Department",
            Instructor = "Test Instructor",
            Schedule = "12:00PM - 1:30PM every day",
            StartDate = new DateOnly(1900, 1, 1),
            EndDate = new DateOnly(1901, 1, 1)
        };
        // Act
        var result = _validator.TestValidate(updateCourseDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCourseDtoValidator_EmptyFields_FailValidation()
    {   
        // Arrange
        var updateCourseDto = new UpdateCourseDto();
        // Act
        var result = _validator.TestValidate(updateCourseDto);
        // Assert
        result.ShouldHaveValidationErrorFor(s => s.CourseName).WithErrorMessage("CourseName is required");
        result.ShouldHaveValidationErrorFor(s => s.Description).WithErrorMessage("CourseDescription is required");
        result.ShouldHaveValidationErrorFor(s => s.Credit).WithErrorMessage("Credit is required");
        result.ShouldHaveValidationErrorFor(s => s.Department).WithErrorMessage("Department is required");
        result.ShouldHaveValidationErrorFor(s => s.Instructor).WithErrorMessage("InstructorName is required");
        result.ShouldHaveValidationErrorFor(s => s.StartDate).WithErrorMessage("StartDate is required");
        result.ShouldHaveValidationErrorFor(s => s.EndDate).WithErrorMessage("EndDate is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(20)]
    public void UpdateCourseDtoValidator_InvalidCredit_FailValidation(int credit)
    {
        // Arrange
        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = "Course Test",
            Description = "This is a course create for testing",
            Credit = credit,
            Department = "Test Department",
            Instructor = "Test Instructor",
            Schedule = "12:00PM - 1:30PM every day",
            StartDate = new DateOnly(1900, 1, 1),
            EndDate = new DateOnly(1901, 1, 1)
        };
        // Act
        var result = _validator.TestValidate(updateCourseDto);
        // Assert
        result.ShouldHaveValidationErrorFor(c=>c.Credit);
    }

    [Fact]
    public void UpdateCourseDtoValidator_FieldsExceed_FailValidation()
    {
        // Arrange
        var updateCourseDto = new UpdateCourseDto
        {
            CourseName = new string('a',101),
            Description = new string('a',501),
            Credit = 21,
            Department = new string('a',51),
            Instructor = new string('a',101),
            StartDate = new DateOnly(1900, 1, 1),
            EndDate = new DateOnly(1901, 1, 1),
            Schedule = new string('a',101)
        };
        // Act
        var result = _validator.TestValidate(updateCourseDto);
        // Assert
        result.ShouldHaveValidationErrorFor(c=>c.CourseName).WithErrorMessage("CourseName should not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(c=>c.Description).WithErrorMessage("CourseDescription should not exceed 500 characters");
        result.ShouldHaveValidationErrorFor(c=>c.Credit).WithErrorMessage("Credit should be less than or equal to 10");
        result.ShouldHaveValidationErrorFor(c=>c.Department).WithErrorMessage("DepartmentName should not exceed 50 characters");
        result.ShouldHaveValidationErrorFor(c=>c.Instructor).WithErrorMessage("InstructorName should not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(c=>c.Schedule).WithErrorMessage("Schedule should not exceed 100 characters");
    }

}

