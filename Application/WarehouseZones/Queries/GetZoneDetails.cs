using Application.Core;
using Application.WarehouseZones.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.WarehouseZones.Queries;

public class GetZoneDetails
{
    public class Query : IRequest<Result<ZoneDetailsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<ZoneDetailsDto>>
    {
        public async Task<Result<ZoneDetailsDto>> Handle(
            Query request,
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

            return Result<ZoneDetailsDto>.Success(mapper.Map<ZoneDetailsDto>(zone));
        }
    }
}
