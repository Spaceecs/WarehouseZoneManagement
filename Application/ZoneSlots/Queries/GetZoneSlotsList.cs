using Application.Core;
using Application.ZoneSlots.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ZoneSlots.Queries;

public class GetZoneSlotsList
{
    public class Query : IRequest<Result<List<ZoneSlotDto>>>
    {
        public Guid ZoneId { get; set; }
        public bool? IsOccupied { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<List<ZoneSlotDto>>>
    {
        public async Task<Result<List<ZoneSlotDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var zoneExists = await context.WarehouseZones.AnyAsync(
                z => z.Id == request.ZoneId,
                cancellationToken
            );

            if (!zoneExists)
                return Result<List<ZoneSlotDto>>.Failure("Zone not found", 404);

            var query = context
                .ZoneSlots.Include(s => s.Pallet)
                .Where(s => s.ZoneId == request.ZoneId)
                .AsNoTracking()
                .AsQueryable();

            if (request.IsOccupied.HasValue)
            {
                query = request.IsOccupied.Value
                    ? query.Where(s => s.Pallet != null)
                    : query.Where(s => s.Pallet == null);
            }

            query = query.OrderBy(s => s.Code);

            var slots = await query
                .ProjectTo<ZoneSlotDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<ZoneSlotDto>>.Success(slots);
        }
    }
}
