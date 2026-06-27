using Domain;

namespace Application.WarehouseZones.DTOs;

public class ZoneListDto : BaseZoneDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = "";
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public bool IsActive { get; set; }
}
