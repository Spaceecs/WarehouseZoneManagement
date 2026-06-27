using Application.WarehouseZones.DTOs;
using FluentValidation;

namespace Application.WarehouseZones.Validators;

public class BaseZoneValidator<T, TDto> : AbstractValidator<T>
    where TDto : BaseZoneDto
{
    public BaseZoneValidator(Func<T, TDto> selector)
    {
        RuleFor(x => selector(x).Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
        RuleFor(x => selector(x).Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");
        RuleFor(x => selector(x).ZoneType).IsInEnum().WithMessage("Invalid zone type");
    }
}
