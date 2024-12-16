using FluentValidation.TestHelper;
using StudentApi.DTOs;
using StudentApi.Validators;

namespace StudentApiTest.Validators;
public class CreateStudentDtoValidatorTest
{
    private readonly CreateStudentDtoValidator _validator;

    public CreateStudentDtoValidatorTest()
    {
        _validator = new CreateStudentDtoValidator();
    }

    [Fact]
    public void CreateStudentDtoValidator_ValidInput_PassValidation()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 2
        };
        // Act
        var result = _validator.TestValidate(createStudentDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateStudentDtoValidator_EmptyFields_FailValidation()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto();

        // Act
        var result = _validator.TestValidate(createStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Email).WithErrorMessage("Email is required");
        result.ShouldHaveValidationErrorFor(s => s.FullName).WithErrorMessage("FullName is required");
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber).WithErrorMessage("PhoneNumber is required");
        result.ShouldHaveValidationErrorFor(s => s.Grade).WithErrorMessage("Grade is required");
        result.ShouldHaveValidationErrorFor(s => s.DateOfBirth).WithErrorMessage("DateOfBirth is required");
    }
    [Fact]
    public void CreateStudentDtoValidator_InvalidEmailFormat_FailValidation()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            Email = "invalid_email",
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 2
        };
        // Act
        var result = _validator.TestValidate(createStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Email).WithErrorMessage("Email is not in a valid format");
    }

    [Fact]
    public void CreateStudentDtoValidator_InvalidPhoneNumberFormat_FailValidation()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            PhoneNumber = "02 -5987244",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 3
        };

        // Act
        var result = _validator.TestValidate(createStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber).WithErrorMessage("PhoneNumber should only contain numeric characters");
    }

    [Fact]
    public void CreateStudentDtoValidator_InvalidDateOfBirth_FailValidation()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            DateOfBirth = DateOnly.MaxValue,
            Grade = 3
        };

        // Act
        var result = _validator.TestValidate(createStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.DateOfBirth).WithErrorMessage("DateOfBirth should be a date in the past");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(-1)]
    public void CreateStudentDtoValidator_GradeOutOfRange_FailValidation(int grade)
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = grade
        };
        // Act
        var result = _validator.TestValidate(createStudentDto);
        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Grade).WithErrorMessage("Grade should be between 1 and 8");
    }

    [Fact]
    public void CreateStudentDtoValidator_MaxLenthExceed_FailValidation()
    {
        // Arrange
        var createStudentDto = new CreateStudentDto
        {
            Email = new string('a', 101),
            FullName = new string('a', 101),
            PhoneNumber = new string('9', 21),
            Address = new string('a', 101),
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 3
        };
        // Act
        var result = _validator.TestValidate(createStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Email).WithErrorMessage("Email should not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber).WithErrorMessage("PhoneNumber should not exceed 20 characters");
        result.ShouldHaveValidationErrorFor(s => s.Address).WithErrorMessage("Address should not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(s => s.FullName).WithErrorMessage("FullName should not exceed 100 characters");
    }
}



