namespace Domain;

public class ZoneSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ZoneId { get; set; }
    public WarehouseZone Zone { get; set; } = null!;
    public required string Code { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Pallet? Pallet { get; set; }
}