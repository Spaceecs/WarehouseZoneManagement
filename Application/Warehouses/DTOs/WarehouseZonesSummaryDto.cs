using System;

namespace Application.Warehouses.DTOs;

public class WarehouseZonesSummaryDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int TotalSlots { get; set; }
    public int OccupiedSlots { get; set; }
}
