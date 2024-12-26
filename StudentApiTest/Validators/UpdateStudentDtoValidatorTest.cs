using FluentValidation.TestHelper;
using StudentApi.DTOs;
using StudentApi.Validators;

namespace StudentApiTest.Validators;
public class UpdateStudentDtoValidatorTest
{
    private readonly UpdateStudentDtoValidator _validator;

    public UpdateStudentDtoValidatorTest()
    {
        _validator = new UpdateStudentDtoValidator();
    }

    [Fact]
    public void UpdateStudentDtoValidator_ValidInput_PassValidation()
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto
        {
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 2
        };
        // Act
        var result = _validator.TestValidate(updateStudentDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateStudentDtoValidator_EmptyFields_FailValidation()
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto{
            StudentId = 0,
            Email = "",
            FullName = "",
            PhoneNumber = "",
            Address = "",
            DateOfBirth = new DateOnly(),
            Grade = 0
        };

        // Act
        var result = _validator.TestValidate(updateStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Email).WithErrorMessage("Email is required");
        result.ShouldHaveValidationErrorFor(s => s.FullName).WithErrorMessage("Full Name is required");
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber).WithErrorMessage("Phone Number is required");
        result.ShouldHaveValidationErrorFor(s => s.Grade).WithErrorMessage("Grade is required");
        result.ShouldHaveValidationErrorFor(s => s.DateOfBirth).WithErrorMessage("Date Of Birth is required");
    }
    [Fact]
    public void UpdateStudentDtoValidator_InvalidEmailFormat_FailValidation()
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto
        {
            Email = "invalid_email",
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 2
        };
        // Act
        var result = _validator.TestValidate(updateStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Email).WithErrorMessage("Email is not in a valid format");
    }

    [Fact]
    public void UpdateStudentDtoValidator_InvalidPhoneNumberFormat_FailValidation()
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            PhoneNumber = "02 -5987244",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 3
        };

        // Act
        var result = _validator.TestValidate(updateStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber).WithErrorMessage("Phone Number should only contain numeric characters");
    }

    [Fact]
    public void UpdateStudentDtoValidator_InvalidDateOfBirth_FailValidation()
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            DateOfBirth = DateOnly.MaxValue,
            Grade = 3
        };

        // Act
        var result = _validator.TestValidate(updateStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.DateOfBirth).WithErrorMessage("Date Of Birth should be a date in the past");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(-1)]
    public void UpdateStudentDtoValidator_GradeOutOfRange_FailValidation(int grade)
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            PhoneNumber = "0333456789",
            Address = "123 Main St",
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = grade
        };
        // Act
        var result = _validator.TestValidate(updateStudentDto);
        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Grade).WithErrorMessage("Grade should be between 1 and 8");
    }

    [Fact]
    public void UpdateStudentDtoValidator_MaxLenthExceed_FailValidation()
    {
        // Arrange
        var updateStudentDto = new UpdateStudentDto
        {
            Email = new string('a', 101),
            FullName = new string('a', 101),
            PhoneNumber = new string('9', 21),
            Address = new string('a', 101),
            DateOfBirth = new DateOnly(1990, 1, 1),
            Grade = 3
        };
        // Act
        var result = _validator.TestValidate(updateStudentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Email).WithErrorMessage("Email should not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(s => s.PhoneNumber).WithErrorMessage("Phone Number should not exceed 20 characters");
        result.ShouldHaveValidationErrorFor(s => s.Address).WithErrorMessage("Address should not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(s => s.FullName).WithErrorMessage("Full Name should not exceed 100 characters");
    }
}



