namespace Application.WarehouseZones.DTOs;

public class ZoneDetailsDto : BaseZoneDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public ZoneWarehouseShortDto Warehouse { get; set; } = null!;
    public ZoneCapacitySummaryDto Capacity { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
