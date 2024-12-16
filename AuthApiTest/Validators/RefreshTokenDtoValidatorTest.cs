using AuthApi.DTOs;
using AuthApi.Resources;
using AuthApi.Validators;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthApiTest.Validators;
public class RefreshTokenDtoValidatorTest
{
    private readonly IStringLocalizer<Resource> _localization;
    private readonly RefreshTokenDtoValidator _validator;

    public RefreshTokenDtoValidatorTest()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        _localization = new StringLocalizer<Resource>(factory);
        _validator = new RefreshTokenDtoValidator(_localization);
    }

    [Fact]
    public void RefreshTokenDtoValidator_ValidRefreshToken_PassValidation()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "test-refresh-token"
        };

        // Act
        var result = _validator.TestValidate(refreshTokenDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RefreshTokenDtoValidator_EmptyRefreshToken_HaveError()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = ""
        };

        // Act
        var result = _validator.TestValidate(refreshTokenDto);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.RefreshToken).WithErrorMessage(_localization[ResourceKey.RefreshTokenRequired]);
    }
}

