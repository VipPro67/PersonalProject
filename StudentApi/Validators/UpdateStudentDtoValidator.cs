﻿using StudentApi.DTOs;
using FluentValidation;

namespace StudentApi.Validators;
public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not in a valid format")
            .MaximumLength(100).WithMessage("Email should not exceed 100 characters")
            .Must(s => !s.Contains('<') && !s.Contains('>') && !s.Contains('&')).WithMessage("Email should not contain HTML tags");
        ;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required")
            .MaximumLength(100).WithMessage("Full Name should not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("Full Name should only contain alphanumeric and spaces")
            .Must(s => !s.Contains('<') && !s.Contains('>') && !s.Contains('&')).WithMessage("Full Name should not contain HTML tags");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number is required")
            .MaximumLength(20).WithMessage("Phone Number should not exceed 20 characters")
            .Matches(@"^\d{10}$").WithMessage("Phone Number should only contain numeric characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date Of Birth is required")
            .LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Date Of Birth should be a date in the past");

        RuleFor(x => x.Grade)
            .NotEmpty().WithMessage("Grade is required")
            .InclusiveBetween(1, 8).WithMessage("Grade should be between 1 and 8");

        RuleFor(x => x.Address)
            .MaximumLength(100).WithMessage("Address should not exceed 100 characters");
    }
}

