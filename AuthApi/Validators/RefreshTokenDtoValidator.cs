using FluentValidation;
using AuthApi.DTOs;
using Microsoft.Extensions.Localization;
using AuthApi.Resources;
using AuthApi.Helpers;

namespace AuthApi.Validators;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    private readonly LocalizationHelper _localizationHelper;

    public RefreshTokenDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localizationHelper = new LocalizationHelper(localization);
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(_localizationHelper.GetComplexMessage(ResourceKey.RefreshToken, ResourceKey.Required));
    }
}
