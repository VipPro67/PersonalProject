using FluentValidation;
using AuthApi.DTOs;
using AuthApi.Resources;
using Microsoft.Extensions.Localization;

namespace AuthApi.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    private readonly IStringLocalizer<Resource> _localization;
    public LoginDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localization = localization;
        RuleFor(x => x.UserName)
            .Must(username => !username.Contains(" ")).WithMessage(_localization[ResourceKey.UsernameNoSpaces])
            .NotEmpty().WithMessage(_localization[ResourceKey.UsernameRequired]);
        RuleFor(x => x.Password)
             .Must(username => !username.Contains(" ")).WithMessage(_localization[ResourceKey.UsernameNoSpaces])
            .NotEmpty().WithMessage(_localization[ResourceKey.PasswordRequired]);
    }
}

