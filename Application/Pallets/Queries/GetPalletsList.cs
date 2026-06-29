using Application.Core;
using Application.Pallets.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Pallets.Queries;

public class GetPalletsList
{
    public class Query : IRequest<Result<PagedList<PalletDto>>>
    {
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<PalletDto>>>
    {
        public async Task<Result<PagedList<PalletDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = context
                .Pallets.Include(p => p.ZoneSlot)
                    .ThenInclude(s => s!.Zone)
                        .ThenInclude(z => z.Warehouse)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                var status = request.Status.Trim().ToLower();
                if (status == "placed")
                {
                    query = query.Where(p => p.ZoneSlotId != null);
                }
                else if (status == "unplaced")
                {
                    query = query.Where(p => p.ZoneSlotId == null);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(p => p.Code.Contains(term));
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var dtoQuery = query.ProjectTo<PalletDto>(mapper.ConfigurationProvider);

            var pagedList = await PagedList<PalletDto>.CreateAsync(
                dtoQuery,
                request.Page,
                request.PageSize
            );

            return Result<PagedList<PalletDto>>.Success(pagedList);
        }
    }
}
