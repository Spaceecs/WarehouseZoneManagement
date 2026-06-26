using System;

namespace Application.Warehouses.DTOs;

public class CreateWarehouseDto : BaseWarehouseDto
{
    public string Code { get; set; } = "";
    public string? Address { get; set; }
}
