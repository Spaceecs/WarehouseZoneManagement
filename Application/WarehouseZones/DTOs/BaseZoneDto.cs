using Domain;

namespace Application.WarehouseZones.DTOs;

public class BaseZoneDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ZoneType ZoneType { get; set; }
}
