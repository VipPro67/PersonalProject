using FluentValidation;
using AuthApi.DTOs;
using AuthApi.Resources;
using Microsoft.Extensions.Localization;
using AuthApi.Helpers;

namespace AuthApi.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    private readonly IStringLocalizer<Resource> _localization;
    public LoginDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localization = localization;
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(_localization[ResourceKey.UsernameRequired]);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localization[ResourceKey.PasswordRequired]);
    }
}

