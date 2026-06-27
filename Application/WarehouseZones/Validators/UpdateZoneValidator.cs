using System;
using Application.WarehouseZones.Commands;
using Application.WarehouseZones.DTOs;
using FluentValidation;

namespace Application.WarehouseZones.Validators;

public class UpdateZoneValidator : BaseZoneValidator<UpdateZone.Command, UpdateZoneDto>
{
    public UpdateZoneValidator()
        : base(x => x.Zone)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Zone id is required");
    }
}
