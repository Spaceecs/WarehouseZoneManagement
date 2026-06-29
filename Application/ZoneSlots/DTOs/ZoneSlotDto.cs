using System;

namespace Application.ZoneSlots.DTOs;

public class ZoneSlotDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public bool IsOccupied { get; set; }
    public ZoneSlotPalletShortDto? Pallet { get; set; }
}
