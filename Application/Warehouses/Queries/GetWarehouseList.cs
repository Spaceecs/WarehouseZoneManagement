using Application.Core;
using Application.Warehouses.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Warehouses.Queries;

public class GetWarehouseList
{
    public class Query : IRequest<Result<PagedList<WarehouseListDto>>>
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<WarehouseListDto>>>
    {
        public async Task<Result<PagedList<WarehouseListDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = context.Warehouses.AsNoTracking();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(searchTerm) || x.Code.ToLower().Contains(searchTerm)
                );
            }

            var dtoQuery = query
                .OrderBy(x => x.Name)
                .ProjectTo<WarehouseListDto>(mapper.ConfigurationProvider);

            var pagedWarehouses = await PagedList<WarehouseListDto>.CreateAsync(
                dtoQuery,
                request.Page,
                request.PageSize
            );

            return Result<PagedList<WarehouseListDto>>.Success(pagedWarehouses);
        }
    }
}
