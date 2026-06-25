namespace Domain;

public enum ZoneType
{
    Receiving,
    Storage,
    Shipping,
    Cold
}

public class WarehouseZone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ZoneType ZoneType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ZoneSlot> Slots { get; set; } = new List<ZoneSlot>();
}