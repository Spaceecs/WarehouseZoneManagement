using Application.Core;
using Application.Pallets.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Pallets.Queries;

public class GetPalletDetails
{
    public class Query : IRequest<Result<PalletDto>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<PalletDto>>
    {
        public async Task<Result<PalletDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var pallet = await context
                .Pallets.Include(p => p.ZoneSlot)
                    .ThenInclude(s => s!.Zone)
                        .ThenInclude(z => z.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (pallet == null)
                return Result<PalletDto>.Failure("Pallet not found", 404);

            return Result<PalletDto>.Success(mapper.Map<PalletDto>(pallet));
        }
    }
}
