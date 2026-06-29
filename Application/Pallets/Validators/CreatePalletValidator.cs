using Application.Pallets.Commands;
using FluentValidation;

namespace Application.Pallets.Validators;

public class CreatePalletValidator : AbstractValidator<CreatePallet.Command>
{
    public CreatePalletValidator()
    {
        RuleFor(x => x.Pallet.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(3, 30)
            .WithMessage("Code must be between 3 and 30 characters")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage("Code must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Pallet.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");
    }
}
