using Application.Core;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ZoneSlots.Commands;

public class DeleteSlot
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid ZoneId { get; set; }
        public Guid SlotId { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var slot = await context
                .ZoneSlots.Include(s => s.Pallet)
                .FirstOrDefaultAsync(s => s.Id == request.SlotId && s.ZoneId == request.ZoneId);

            if (slot == null)
                return Result<Unit>.Failure("Slot not found", 404);

            if (slot.Pallet != null)
                return Result<Unit>.Failure(
                    "Cannot delete a slot that is occupied by a pallet",
                    400
                );

            context.ZoneSlots.Remove(slot);

            var zone = await context.WarehouseZones.FindAsync([request.ZoneId], cancellationToken);
            zone?.UpdatedAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<Unit>.Failure("Failed to delete the slot", 500);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
