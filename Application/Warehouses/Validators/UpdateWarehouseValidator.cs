using Application.Warehouses.Commands;
using Application.Warehouses.DTOs;
using FluentValidation;

namespace Application.Warehouses.Validators;

public class UpdateWarehouseValidator
    : BaseWarehouseValidator<UpdateWarehouse.Command, UpdateWarehouseDto>
{
    public UpdateWarehouseValidator()
        : base(x => x.Warehouse)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Warehouse Id is required");
        RuleFor(x => x.Warehouse.Address)
            .MaximumLength(200)
            .WithMessage("Address cannot exceed 200 characters");
    }
}
