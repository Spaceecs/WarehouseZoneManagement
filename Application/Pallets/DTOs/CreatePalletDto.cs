namespace Application.Pallets.DTOs;

public class CreatePalletDto
{
    public required string Code { get; set; }
    public string? Description { get; set; }
}
