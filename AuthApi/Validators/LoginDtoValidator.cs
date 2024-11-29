using FluentValidation;
using AuthApi.DTOs;
using Microsoft.Extensions.Localization;
using Serilog;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace AuthApi.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    private readonly IStringLocalizer<LoginDto> _localizer;

    public LoginDtoValidator(IStringLocalizer<LoginDto> localizer, IStringLocalizerFactory factory)
    {
        _localizer = localizer;
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(x => localizer["Username is required"])
            .MaximumLength(50).WithMessage(x => localizer["Username must be at most 50 characters long"])
            .Matches("^[a-zA-Z0-9]+$").WithMessage(x => localizer["Username must contain only alphanumeric characters"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(x => localizer["Password is required"])
            .MinimumLength(8).WithMessage(x => localizer["Password must be at least 8 characters long"])
            .Matches("[a-z]").WithMessage(x => localizer["Password must contain at least one lowercase letter"])
            .Matches("[A-Z]").WithMessage(x => localizer["Password must contain at least one uppercase letter"])
            .Matches("[0-9]").WithMessage(x => localizer["Password must contain at least one digit"])
            .Matches("[^a-zA-Z0-9]").WithMessage(x => localizer["Password must contain at least one special character"]);
    }
}
