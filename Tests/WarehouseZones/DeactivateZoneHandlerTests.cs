using Application.WarehouseZones.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.WarehouseZones;

public class DeactivateZoneHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenZoneHasOccupiedSlots()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-010",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEX",
            Name = "Zone X",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEX-S001" };
        var pallet = new Pallet { Code = "PLT-010", ZoneSlotId = slot.Id };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new DeactivateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new DeactivateZone.Command { Id = zone.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal(
            "Cannot deactivate a zone that contains occupied slots with pallets",
            result.Error
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateZone_WhenNoOccupiedSlots()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-011",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEY",
            Name = "Zone Y",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new DeactivateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new DeactivateZone.Command { Id = zone.Id },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsActive);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenZoneAlreadyInactive()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-012",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEZ",
            Name = "Zone Z",
            ZoneType = ZoneType.Storage,
            IsActive = false,
        };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new DeactivateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new DeactivateZone.Command { Id = zone.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Zone already deactivated", result.Error);
    }
}
