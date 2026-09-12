using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

namespace LabConsumableExpiryTracker.Application.Validators.User;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Username can only contain letters and numbers.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full Name can only contain letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .Must(email => email.EndsWith("@formulatrix.com"))
            .WithMessage("Email must use a Formulatrix email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be either WarehouseAdmin or Scientist.");
    }
}