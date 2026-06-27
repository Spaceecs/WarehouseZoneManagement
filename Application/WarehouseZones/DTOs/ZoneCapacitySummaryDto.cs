using System;

namespace Application.WarehouseZones.DTOs;

public class ZoneCapacitySummaryDto
{
    public int Total { get; set; }
    public int Occupied { get; set; }
    public int Free { get; set; }
    public double UtilizationPercent { get; set; }
}
