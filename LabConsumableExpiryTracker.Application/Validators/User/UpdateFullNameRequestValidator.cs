using FluentValidation;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

namespace LabConsumableExpiryTracker.Application.Validators.Users;

public class UpdateFullNameRequestValidator : AbstractValidator<UpdateFullNameRequestDto>
{
    public UpdateFullNameRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Nama lengkap wajib diisi.")
            .MaximumLength(100)
            .WithMessage("Nama lengkap tidak boleh lebih dari 100 karakter.")
            .Matches(@"^[a-zA-Z0-9\s\.\-]+$") 
            .WithMessage("Nama lengkap hanya boleh mengandung huruf, angka, spasi, titik, dan strip.");
    }
}