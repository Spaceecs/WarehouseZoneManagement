using Application.Warehouses.DTOs;
using FluentValidation;

namespace Application.Warehouses.Validators;

public class BaseWarehouseValidator<T, TDto> : AbstractValidator<T>
    where TDto : BaseWarehouseDto
{
    public BaseWarehouseValidator(Func<T, TDto> selector)
    {
        RuleFor(x => selector(x).Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
    }
}
