using FluentValidation;
using AuthApi.DTOs;
using Microsoft.Extensions.Localization;
using AuthApi.Resources;

namespace AuthApi.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{

    private readonly IStringLocalizer<Resource> _localization;

    public RegisterDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localization = localization;
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(_localization[ResourceKey.UsernameRequired])
            .MaximumLength(50).WithMessage(_localization[ResourceKey.UsernameMaxLength, "50"])
            .Matches("^[a-zA-Z0-9]+$").WithMessage(_localization[ResourceKey.UsernameAlphanumeric]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localization[ResourceKey.PasswordRequired])
            .MinimumLength(8).WithMessage(_localization[ResourceKey.PasswordMinLength, "8"])
            .Matches("[a-z]").WithMessage(_localization[ResourceKey.PasswordMustContainLowercase])
            .Matches("[A-Z]").WithMessage(_localization[ResourceKey.PasswordMustContainUppercase])
            .Matches("[0-9]").WithMessage(_localization[ResourceKey.PasswordMustContainDigit])
            .Matches("[^a-zA-Z0-9]").WithMessage(_localization[ResourceKey.PasswordMustContainSpecialCharacter]);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_localization[ResourceKey.EmailRequired])
            .EmailAddress().WithMessage(_localization[ResourceKey.EmailInvalid]);
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(_localization[ResourceKey.FullNameRequired])
            .MaximumLength(100).WithMessage(_localization[ResourceKey.FullNameMaxLength, "100"]);
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(_localization[ResourceKey.AddressRequired])
            .MaximumLength(200).WithMessage(_localization[ResourceKey.AddressMaxLength, "200"]);
    }
}
