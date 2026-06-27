using System;
using Application.Core;
using Application.WarehouseZones.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.WarehouseZones.Commands;

public class DeactivateZone
{
    public class Command : IRequest<Result<ZoneDetailsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<ZoneDetailsDto>>
    {
        public async Task<Result<ZoneDetailsDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var zone = await context
                .WarehouseZones.Include(z => z.Warehouse)
                .Include(z => z.Slots)
                    .ThenInclude(sl => sl.Pallet)
                .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);

            if (zone == null)
                return Result<ZoneDetailsDto>.Failure("Zone not found", 404);

            if (!zone.IsActive)
                return Result<ZoneDetailsDto>.Failure("Zone already deactivated", 400);

            var hasOccupiedSlots = zone.Slots.Any(sl => sl.Pallet != null);

            if (hasOccupiedSlots)
                return Result<ZoneDetailsDto>.Failure(
                    "Cannot deactivate a zone that contains occupied slots with pallets",
                    400
                );

            zone.IsActive = false;
            zone.UpdatedAt = DateTime.UtcNow;

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<ZoneDetailsDto>.Failure(
                    "No changes were made or failed to update zone",
                    400
                );

            return Result<ZoneDetailsDto>.Success(mapper.Map<ZoneDetailsDto>(zone));
        }
    }
}
