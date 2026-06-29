using Application.Core;
using Application.Pallets.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Pallets.Commands;

public class RemovePallet
{
    public class Command : IRequest<Result<PalletDto>>
    {
        public Guid PalletId { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<PalletDto>>
    {
        public async Task<Result<PalletDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var pallet = await context.Pallets.FirstOrDefaultAsync(
                p => p.Id == request.PalletId,
                cancellationToken
            );

            if (pallet == null)
                return Result<PalletDto>.Failure("Pallet not found", 404);

            if (!pallet.ZoneSlotId.HasValue)
                return Result<PalletDto>.Failure("Pallet is already unplaced", 400);

            pallet.ZoneSlotId = null;
            pallet.UpdatedAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<PalletDto>.Failure("Failed to remove pallet from slot", 500);

            return Result<PalletDto>.Success(mapper.Map<PalletDto>(pallet));
        }
    }
}
