using Application.Core;
using Application.Warehouses.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Warehouses.Commands;

public class DeactivateWarehouse
{
    public class Command : IRequest<Result<WarehouseDetailsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<WarehouseDetailsDto>>
    {
        public async Task<Result<WarehouseDetailsDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var warehouse = await context
                .Warehouses.Include(w => w.Zones)
                    .ThenInclude(z => z.Slots)
                        .ThenInclude(s => s.Pallet)
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            if (warehouse == null)
                return Result<WarehouseDetailsDto>.Failure("Warehouse not found", 404);

            if (!warehouse.IsActive)
                return Result<WarehouseDetailsDto>.Failure("Warehouse already deactivated", 400);

            warehouse.IsActive = false;
            warehouse.UpdatedAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<WarehouseDetailsDto>.Failure(
                    "No changes were made or failed to update warehouse",
                    400
                );

            return Result<WarehouseDetailsDto>.Success(mapper.Map<WarehouseDetailsDto>(warehouse));
        }
    }
}
