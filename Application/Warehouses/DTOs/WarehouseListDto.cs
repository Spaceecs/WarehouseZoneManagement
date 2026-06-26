namespace Application.Warehouses.DTOs;

public class WarehouseListDto : BaseWarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public int TotalZones { get; set; }
    public int ActiveZones { get; set; }
}
