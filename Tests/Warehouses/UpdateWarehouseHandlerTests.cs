using System;
using Application.Warehouses.Commands;
using Application.Warehouses.DTOs;
using Domain;
using Tests.TestHelpers;

namespace Tests.Warehouses;

public class UpdateWarehouseHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCorrectSlotCount_AfterUpdate()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-020",
            Name = "Main",
            IsActive = true,
        };

        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEA",
            Name = "Zone A",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var slot1 = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEA-S001" };
        var slot2 = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEA-S002" };
        var pallet = new Pallet { Code = "PLT-020", ZoneSlotId = slot1.Id };
        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.AddRange(slot1, slot2);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new UpdateWarehouse.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new UpdateWarehouse.Command
            {
                Id = warehouse.Id,
                Warehouse = new UpdateWarehouseDto { Name = "Main Updated", Address = "Kyiv" },
            },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Zones.TotalSlots);
        Assert.Equal(1, result.Value.Zones.OccupiedSlots);
    }
}
