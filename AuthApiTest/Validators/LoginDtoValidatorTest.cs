using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Resources;
using AuthApi.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AuthApiTest.Validators;
public class LoginDtoValidatorTest
{
    private readonly IStringLocalizer<Resource> _localization;
    private readonly LoginDtoValidator _validator;

    public LoginDtoValidatorTest()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        _localization = new StringLocalizer<Resource>(factory);
        _validator = new LoginDtoValidator(_localization);
    }

    [Fact]
    public void LoginDtoValidator_ValidCredentials_PassValidation()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "Password123!",
        };

        // Act
        var result = _validator.TestValidate(loginDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginDtoValidator_EmptyUsername_FailValidation()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "",
            Password = "Password123!",
        };

        // Act
        var result = _validator.TestValidate(loginDto);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.UserName).WithErrorMessage(_localization[ResourceKey.UsernameRequired]);
    }

    [Fact]
    public void LoginDtoValidator_EmptyPassword_FailValidation()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "testuser",
            Password = "",
        };
        // Act
        var result = _validator.TestValidate(loginDto);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Password).WithErrorMessage(_localization[ResourceKey.PasswordRequired]);
    }

    [Fact]
    public void LoginDtoValidator_EmptyFields_FailValidation()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UserName = "",
            Password = "",
        };

        // Act
        var result = _validator.TestValidate(loginDto);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.UserName).WithErrorMessage(_localization[ResourceKey.UsernameRequired]);
        result.ShouldHaveValidationErrorFor(r => r.Password).WithErrorMessage(_localization[ResourceKey.PasswordRequired]);
    }

}
