using Application.Core;
using Application.Pallets.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Pallets.Commands;

public class MovePallet
{
    public class Command : IRequest<Result<PalletDto>>
    {
        public Guid PalletId { get; set; }
        public Guid TargetSlotId { get; set; }
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
                return Result<PalletDto>.Failure("Pallet is unplaced. Use Place instead", 400);

            var targetSlot = await context
                .ZoneSlots.Include(s => s.Pallet)
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == request.TargetSlotId, cancellationToken);

            if (targetSlot == null)
                return Result<PalletDto>.Failure("Target slot not found", 404);

            if (targetSlot.Pallet != null)
                return Result<PalletDto>.Failure("Target slot is already occupied", 400);

            if (!targetSlot.Zone.IsActive)
                return Result<PalletDto>.Failure("Cannot move pallet to an inactive zone", 400);

            if (!targetSlot.Zone.Warehouse.IsActive)
                return Result<PalletDto>.Failure(
                    "Cannot move pallet because the destination warehouse is inactive",
                    400
                );

            pallet.ZoneSlotId = targetSlot.Id;
            pallet.UpdatedAt = DateTime.UtcNow;
            targetSlot.Zone.UpdatedAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<PalletDto>.Failure("Failed to move pallet", 500);

            await context.Entry(pallet).Reference(p => p.ZoneSlot).LoadAsync(cancellationToken);
            await context
                .Entry(pallet.ZoneSlot!)
                .Reference(s => s.Zone)
                .LoadAsync(cancellationToken);
            await context
                .Entry(pallet.ZoneSlot!.Zone)
                .Reference(z => z.Warehouse)
                .LoadAsync(cancellationToken);

            return Result<PalletDto>.Success(mapper.Map<PalletDto>(pallet));
        }
    }
}
