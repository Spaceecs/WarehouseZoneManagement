namespace Domain;

public class Pallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Code { get; set; }
    public string? Description { get; set; }
    public Guid? ZoneSlotId { get; set; }
    public ZoneSlot? ZoneSlot { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}