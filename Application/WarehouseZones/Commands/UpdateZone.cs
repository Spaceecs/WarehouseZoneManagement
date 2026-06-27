using System;
using Application.Core;
using Application.WarehouseZones.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.WarehouseZones.Commands;

public class UpdateZone
{
    public class Command : IRequest<Result<ZoneDetailsDto>>
    {
        public Guid Id { get; set; }
        public required UpdateZoneDto Zone { get; set; }
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
                .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);

            if (zone == null)
                return Result<ZoneDetailsDto>.Failure("Zone not found", 404);

            zone.Name = request.Zone.Name;
            zone.Description = request.Zone.Description;
            zone.ZoneType = request.Zone.ZoneType;
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
