using FluentValidation;
using AuthApi.DTOs;
using AuthApi.Resources;
using Microsoft.Extensions.Localization;
using AuthApi.Helpers;

namespace AuthApi.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    private readonly LocalizationHelper _localizationHelper;
    public LoginDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localizationHelper = new LocalizationHelper(localization);
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Username, ResourceKey.Required));
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.Required));
    }
}

