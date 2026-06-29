using Application.Core;
using Application.Warehouses.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Warehouses.Queries;

public class GetWarehouseDetails
{
    public class Query : IRequest<Result<WarehouseDetailsDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<WarehouseDetailsDto>>
    {
        public async Task<Result<WarehouseDetailsDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var warehouse = await context
                .Warehouses.AsNoTracking()
                .Where(x => x.Id == request.Id)
                .ProjectTo<WarehouseDetailsDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (warehouse == null)
                return Result<WarehouseDetailsDto>.Failure("Warehouse not found", 404);

            return Result<WarehouseDetailsDto>.Success(warehouse);
        }
    }
}
