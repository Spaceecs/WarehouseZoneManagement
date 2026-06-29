namespace Application.Warehouses.DTOs;

public class WarehouseDetailsDto : BaseWarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string? Address { get; set; }
    public WarehouseZonesSummaryDto Zones { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
