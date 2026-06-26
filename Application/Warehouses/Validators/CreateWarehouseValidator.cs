using Application.Warehouses.Commands;
using Application.Warehouses.DTOs;
using FluentValidation;

namespace Application.Warehouses.Validators;

public class CreateWarehouseValidator
    : BaseWarehouseValidator<CreateWarehouse.Command, CreateWarehouseDto>
{
    public CreateWarehouseValidator()
        : base(x => x.Warehouse)
    {
        RuleFor(x => x.Warehouse.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(3, 20)
            .WithMessage("Code must be between 3 and 20 characters")
            .Matches("^[A-Z0-9-]+$")
            .WithMessage("Code must contain only uppercase letters, numbers, and hyphens");
        RuleFor(x => x.Warehouse.Address)
            .MaximumLength(200)
            .WithMessage("Address cannot exceed 200 characters");
    }
}
