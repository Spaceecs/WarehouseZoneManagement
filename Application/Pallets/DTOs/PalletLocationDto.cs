namespace Application.Pallets.DTOs;

public class PalletLocationDto
{
    public Guid SlotId { get; set; }
    public required string SlotCode { get; set; }
    public Guid ZoneId { get; set; }
    public required string ZoneCode { get; set; }
    public required string ZoneName { get; set; }
    public Guid WarehouseId { get; set; }
    public required string WarehouseName { get; set; }
}
