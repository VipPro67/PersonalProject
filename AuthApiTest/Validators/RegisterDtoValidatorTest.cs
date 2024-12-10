using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Resources;
using AuthApi.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace AuthApiTest.Validators
{
    public class RegisterDtoValidatorTest
    {
        private readonly Mock<IStringLocalizer<Resource>> _mockLocalization;
        private readonly RegisterDtoValidator _validator;

        public RegisterDtoValidatorTest()
        {
            _mockLocalization = new Mock<IStringLocalizer<Resource>>();
            _validator = new RegisterDtoValidator(_mockLocalization.Object);
        }

    }
}
