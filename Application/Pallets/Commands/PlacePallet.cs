using Application.Core;
using Application.Pallets.DTOs;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Pallets.Commands;

public class PlacePallet
{
    public class Command : IRequest<Result<PalletDto>>
    {
        public Guid PalletId { get; set; }
        public Guid SlotId { get; set; }
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

            var slot = await context
                .ZoneSlots.Include(s => s.Pallet)
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);

            if (slot == null)
                return Result<PalletDto>.Failure("Slot not found", 404);

            if (slot.Pallet != null)
                return Result<PalletDto>.Failure("Slot is already occupied", 400);

            if (!slot.Zone.IsActive)
                return Result<PalletDto>.Failure("Cannot place pallet in an inactive zone", 400);

            if (!slot.Zone.Warehouse.IsActive)
                return Result<PalletDto>.Failure(
                    "Cannot place pallet because the warehouse is inactive",
                    400
                );

            pallet.ZoneSlotId = slot.Id;
            pallet.UpdatedAt = DateTime.UtcNow;
            slot.Zone.UpdatedAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<PalletDto>.Failure("Failed to place pallet", 500);

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
