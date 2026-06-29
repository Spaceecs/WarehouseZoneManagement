using Application.Core;
using Application.WarehouseZones.DTOs;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.WarehouseZones.Commands;

public class CreateZone
{
    public class Command : IRequest<Result<ZoneDetailsDto>>
    {
        public required CreateZoneDto Zone { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<ZoneDetailsDto>>
    {
        public async Task<Result<ZoneDetailsDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var warehouse = await context.Warehouses.FirstOrDefaultAsync(
                w => w.Id == request.Zone.WarehouseId,
                cancellationToken
            );

            if (warehouse == null)
                return Result<ZoneDetailsDto>.Failure("Warehouse is not found", 404);

            if (!warehouse.IsActive)
                return Result<ZoneDetailsDto>.Failure(
                    "Cannot create a zone in an inactive warehouse",
                    400
                );

            var codeExists = await context.WarehouseZones.AnyAsync(
                z => z.WarehouseId == request.Zone.WarehouseId && z.Code == request.Zone.Code,
                cancellationToken
            );

            if (codeExists)
                return Result<ZoneDetailsDto>.Failure(
                    "Zone code must be unique within this warehouse",
                    400
                );

            var zone = mapper.Map<WarehouseZone>(request.Zone);
            zone.IsActive = true;
            zone.CreatedAt = DateTime.UtcNow;
            zone.UpdatedAt = DateTime.UtcNow;

            context.WarehouseZones.Add(zone);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<ZoneDetailsDto>.Failure("Failed to create zone", 500);

            zone.Warehouse = warehouse;
            return Result<ZoneDetailsDto>.Success(mapper.Map<ZoneDetailsDto>(zone));
        }
    }
}
