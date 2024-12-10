using FluentValidation;
using AuthApi.DTOs;
using Microsoft.Extensions.Localization;
using AuthApi.Resources;
using AuthApi.Helpers;

namespace AuthApi.Validators;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    private readonly IStringLocalizer<Resource> _localization;

    public RefreshTokenDtoValidator(IStringLocalizer<Resource> localization)
    {
        _localization = localization;
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(_localization[ResourceKey.RefreshTokenRequired]);
    }
}
