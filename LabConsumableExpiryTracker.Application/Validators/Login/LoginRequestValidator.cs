using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;

namespace LabConsumableExpiryTracker.Application.Validators.Login;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}