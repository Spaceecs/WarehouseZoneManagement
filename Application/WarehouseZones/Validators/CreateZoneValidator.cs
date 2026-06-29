using Application.WarehouseZones.Commands;
using Application.WarehouseZones.DTOs;
using FluentValidation;

namespace Application.WarehouseZones.Validators;

public class CreateZoneValidator : BaseZoneValidator<CreateZone.Command, CreateZoneDto>
{
    public CreateZoneValidator()
        : base(x => x.Zone)
    {
        RuleFor(x => x.Zone.WarehouseId).NotEmpty().WithMessage("WarehouseId is required");

        RuleFor(x => x.Zone.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(3, 20)
            .WithMessage("Code must be between 3 and 20 characters")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage("Code must contain only uppercase letters, numbers, and hyphens");
    }
}
