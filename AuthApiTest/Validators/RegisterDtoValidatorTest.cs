using AuthApi.DTOs;
using AuthApi.Resources;
using AuthApi.Validators;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthApiTest.Validators;
public class RegisterDtoValidatorTest
{
    private readonly IStringLocalizer<Resource> _localization;
    private readonly RegisterDtoValidator _validator;

    public RegisterDtoValidatorTest()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        _localization = new StringLocalizer<Resource>(factory);
        _validator = new RegisterDtoValidator(_localization);
    }

    [Fact]
    public void RegisterDtoValidator_ValidCredentials_PassValidation()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Password = "Password123!",
            Email = "testuser@example.com",
            Address = "123 Main St",
            FullName = "Test User"
        };

        // Act
        var result = _validator.TestValidate(registerDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterDtoValidator_EmptyFields_FailValidation()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "",
            Password = "",
            Email = "",
            Address = "",
            FullName = ""
        };

        // Act
        var result = _validator.TestValidate(registerDto);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.UserName).WithErrorMessage(_localization[ResourceKey.UsernameRequired]);
        result.ShouldHaveValidationErrorFor(r => r.Password).WithErrorMessage(_localization[ResourceKey.PasswordRequired]);
        result.ShouldHaveValidationErrorFor(r => r.Email).WithErrorMessage(_localization[ResourceKey.EmailRequired]);
        result.ShouldHaveValidationErrorFor(r => r.Address).WithErrorMessage(_localization[ResourceKey.AddressRequired]);
        result.ShouldHaveValidationErrorFor(r => r.FullName).WithErrorMessage(_localization[ResourceKey.FullNameRequired]);
    }

    [Fact]
    public void RegisterDtoValidator_MinLenthPassword_FailValidation()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Password = "P",
            Email = "testuser@example.com",
            Address = "123 Main St",
            FullName = "Test User"
        };
        // Act
        var result = _validator.TestValidate(registerDto);
        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Password).WithErrorMessage(_localization[ResourceKey.PasswordMinLength]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("testuser@example")]
    [InlineData("testuser@example.")]
    [InlineData("testuser@")]
    public void RegisterDtoValidator_InvalidUserName_FailValidation(string username)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = username,
            Password = "Password123!",
            Email = "testuser@example.com",
            Address = "123 Main St",
            FullName = "Test User"
        };

        // Act

        var result = _validator.TestValidate(registerDto);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.UserName);
    }
    [Theory]
    [InlineData("pasword")]
    [InlineData("pas word")]
    [InlineData("p@ssword")]
    [InlineData("password123")]
    [InlineData("Password123")]
    public void RegisterDtoValidator_InvalidPassword_FailValidation(string password)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Password = password,
            Email = "testuser@example.com",
            Address = "123 Main St",
            FullName = "Test User"
        };
        // Act
        var result = _validator.TestValidate(registerDto);
        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("testuserexample.com")]
    [InlineData("testu serexample.com")]
    public void RegisterDtoValidator_InvalidEmail_FailValidation(string email)
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "testuser",
            Password = "Password123!",
            Email = email,
            Address = "123 Main St",
            FullName = "Test User"
        };
        // Act
        var result = _validator.TestValidate(registerDto);
        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Fact]
    public void RegisterDtoValidator_MaxLenthExceed_FailValidation()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = new string('a', 51),
            Password = "Password123!",
            Email = "testuser@example.com",
            Address = new string('a', 201),
            FullName = new string('a', 101),
        };

        // Act
        var result = _validator.TestValidate(registerDto);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.UserName).WithErrorMessage(_localization[ResourceKey.UsernameMaxLength]);
        result.ShouldHaveValidationErrorFor(u => u.Address).WithErrorMessage(_localization[ResourceKey.AddressMaxLength]);
        result.ShouldHaveValidationErrorFor(u => u.FullName).WithErrorMessage(_localization[ResourceKey.FullNameMaxLength]);
    }

}
