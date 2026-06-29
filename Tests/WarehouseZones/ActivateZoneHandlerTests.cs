using Application.WarehouseZones.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.WarehouseZones;

public class ActivateZoneHandlerTests
{
    [Fact]
    public async Task Handle_ShouldActivateZone_WhenInactive()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-060",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEAA",
            Name = "Zone AA",
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
        var handler = new ActivateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new ActivateZone.Command { Id = zone.Id },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsActive);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenZoneIsAlreadyActive()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-061",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEBB",
            Name = "Zone BB",
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
        var handler = new ActivateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new ActivateZone.Command { Id = zone.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Zone is already active", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenZoneDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new ActivateZone.Handler(context, MapperFactory.Create());

        var result = await handler.Handle(
            new ActivateZone.Command { Id = Guid.NewGuid() },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Zone not found", result.Error);
    }
}
