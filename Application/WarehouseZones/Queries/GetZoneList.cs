using Application.Core;
using Application.WarehouseZones.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.WarehouseZones.Queries;

public class GetZoneList
{
    public class Query : IRequest<Result<PagedList<ZoneListDto>>>
    {
        public Guid? WarehouseId { get; set; }
        public ZoneType? ZoneType { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<ZoneListDto>>>
    {
        public async Task<Result<PagedList<ZoneListDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = context
                .WarehouseZones.Include(z => z.Warehouse)
                .Include(z => z.Slots)
                    .ThenInclude(sl => sl.Pallet)
                .AsNoTracking()
                .AsQueryable();

            if (request.WarehouseId.HasValue)
            {
                query = query.Where(z => z.WarehouseId == request.WarehouseId.Value);
            }

            if (request.ZoneType.HasValue)
            {
                query = query.Where(z => z.ZoneType == request.ZoneType.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(z => z.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(z => z.Name.Contains(term) || z.Code.Contains(term));
            }

            query = query.OrderByDescending(z => z.CreatedAt);

            var dtoQuery = query.ProjectTo<ZoneListDto>(mapper.ConfigurationProvider);

            var pagedList = await PagedList<ZoneListDto>.CreateAsync(
                dtoQuery,
                request.Page,
                request.PageSize
            );

            return Result<PagedList<ZoneListDto>>.Success(pagedList);
        }
    }
}
