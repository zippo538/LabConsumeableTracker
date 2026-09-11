using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.Scientist;

namespace LabConsumableExpiryTracker.Application.Validators.Scientist;

public class UpdateScientistRequestValidator
    : AbstractValidator<UpdateScientistRequestDto>
{
    public UpdateScientistRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("Username can only contain letters and numbers.");
        
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full Name is required.")
            .MaximumLength(100)
            .WithMessage("Full Name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("Full Name can only contain letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .Must(email => email.EndsWith("@formulatrix.com"))
            .WithMessage("Email must use a Formulatrix email address.");
    }
}