namespace Application.WarehouseZones.DTOs;

public class CreateZoneDto : BaseZoneDto
{
    public Guid WarehouseId { get; set; }
    public required string Code { get; set; }
}
