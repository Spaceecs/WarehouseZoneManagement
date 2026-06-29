using System;
using Application.Core;
using Application.ZoneSlots.DTOs;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ZoneSlots.Commands;

public class BulkCreateSlots
{
    public class Command : IRequest<Result<List<ZoneSlotDto>>>
    {
        public Guid ZoneId { get; set; }
        public int Count { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<List<ZoneSlotDto>>>
    {
        public async Task<Result<List<ZoneSlotDto>>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            if (request.Count <= 0 || request.Count > 100)
                return Result<List<ZoneSlotDto>>.Failure("Count must be between 1 and 100", 400);

            var zone = await context
                .WarehouseZones.Include(z => z.Slots)
                .FirstOrDefaultAsync(z => z.Id == request.ZoneId, cancellationToken);

            if (zone == null)
                return Result<List<ZoneSlotDto>>.Failure("Zone not found", 404);

            if (!zone.IsActive)
                return Result<List<ZoneSlotDto>>.Failure(
                    "Cannot generate slots for an inactive zone",
                    400
                );

            int currentMaxIndex = 0;
            foreach (var slot in zone.Slots)
            {
                var parts = slot.Code.Split("-S");
                if (parts.Length > 1 && int.TryParse(parts[^1], out int index))
                {
                    if (index > currentMaxIndex)
                        currentMaxIndex = index;
                }
            }

            var newSlots = new List<ZoneSlot>();

            for (int i = 1; i <= request.Count; i++)
            {
                int nextIndex = currentMaxIndex + i;
                string slotCode = $"{zone.Code}-S{nextIndex:D3}";

                newSlots.Add(new ZoneSlot { ZoneId = zone.Id, Code = slotCode });
            }

            await context.ZoneSlots.AddRangeAsync(newSlots, cancellationToken);
            zone.UpdatedAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<List<ZoneSlotDto>>.Failure("Failed to save slots", 500);

            var dtos = mapper.Map<List<ZoneSlotDto>>(newSlots);

            return Result<List<ZoneSlotDto>>.Success(dtos);
        }
    }
}
