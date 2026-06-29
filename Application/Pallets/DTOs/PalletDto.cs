namespace Application.Pallets.DTOs;

public class PalletDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public required string Status { get; set; }
    public PalletLocationDto? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
