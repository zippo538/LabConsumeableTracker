using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;

namespace LabConsumableExpiryTracker.Application.Validators.Login;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}