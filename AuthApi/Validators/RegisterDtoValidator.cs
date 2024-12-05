using FluentValidation;
using AuthApi.DTOs;
using Microsoft.Extensions.Localization;
using AuthApi.Resources;
using AuthApi.Helpers;

namespace AuthApi.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    private readonly LocalizationHelper _localizationHelper;

    public RegisterDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localizationHelper = new LocalizationHelper(localization);
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Username, ResourceKey.Required))
            .MaximumLength(50).WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Username, ResourceKey.MaxLength, 50))
            .Matches("^[a-zA-Z0-9]+$").WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Username, ResourceKey.Alphanumeric));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.Required))
            .MinimumLength(8).WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.MinLength, 8))
            .Matches("[a-z]").WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.MustContainLowercase))
            .Matches("[A-Z]").WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.MustContainUppercase))
            .Matches("[0-9]").WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.MustContainDigit))
            .Matches("[^a-zA-Z0-9]").WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Password, ResourceKey.MustContainSpecialCharacter));
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Email, ResourceKey.Required))
            .EmailAddress().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Email, ResourceKey.Invalid));
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.FullName,ResourceKey.Required))
            .MaximumLength(100).WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.FullName, ResourceKey.MaxLength, 100));
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Address, ResourceKey.Required))
            .MaximumLength(200).WithMessage(_localizationHelper.GetLocalizedMessage(ResourceKey.Address, ResourceKey.MaxLength, 200));
    }
}
