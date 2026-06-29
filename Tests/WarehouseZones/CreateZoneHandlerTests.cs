using Application.WarehouseZones.Commands;
using Application.WarehouseZones.DTOs;
using Domain;
using Tests.TestHelpers;

namespace Tests.WarehouseZones;

public class CreateZoneHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateZone_WhenWarehouseActiveAndCodeUnique()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-070",
            Name = "Main",
            IsActive = true,
        };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new CreateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new CreateZone.Command
            {
                Zone = new CreateZoneDto
                {
                    WarehouseId = warehouse.Id,
                    Code = "ZONECC",
                    Name = "Zone CC",
                    ZoneType = ZoneType.Cold,
                },
            },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsActive);
        Assert.Equal("ZONECC", result.Value.Code);
        Assert.Equal(warehouse.Code, result.Value.Warehouse.Code);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenWarehouseDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateZone.Handler(context, MapperFactory.Create());

        var result = await handler.Handle(
            new CreateZone.Command
            {
                Zone = new CreateZoneDto
                {
                    WarehouseId = Guid.NewGuid(),
                    Code = "ZONEDD",
                    Name = "Zone DD",
                    ZoneType = ZoneType.Storage,
                },
            },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Warehouse is not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenWarehouseIsInactive()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-071",
            Name = "Main",
            IsActive = false,
        };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new CreateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new CreateZone.Command
            {
                Zone = new CreateZoneDto
                {
                    WarehouseId = warehouse.Id,
                    Code = "ZONEEE",
                    Name = "Zone EE",
                    ZoneType = ZoneType.Storage,
                },
            },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot create a zone in an inactive warehouse", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeIsDuplicateWithinWarehouse()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-072",
            Name = "Main",
            IsActive = true,
        };
        var existingZone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEFF",
            Name = "Zone FF",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(existingZone);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new CreateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new CreateZone.Command
            {
                Zone = new CreateZoneDto
                {
                    WarehouseId = warehouse.Id,
                    Code = "ZONEFF",
                    Name = "Another zone with same code",
                    ZoneType = ZoneType.Storage,
                },
            },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Zone code must be unique within this warehouse", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldAllowSameCode_InDifferentWarehouses()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouseOne = new Warehouse
        {
            Code = "WH-073",
            Name = "Main One",
            IsActive = true,
        };
        var warehouseTwo = new Warehouse
        {
            Code = "WH-074",
            Name = "Main Two",
            IsActive = true,
        };
        var zoneInWarehouseOne = new WarehouseZone
        {
            WarehouseId = warehouseOne.Id,
            Code = "ZONEGG",
            Name = "Zone GG",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.AddRange(warehouseOne, warehouseTwo);
            arrange.WarehouseZones.Add(zoneInWarehouseOne);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new CreateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new CreateZone.Command
            {
                Zone = new CreateZoneDto
                {
                    WarehouseId = warehouseTwo.Id,
                    Code = "ZONEGG",
                    Name = "Zone GG in another warehouse",
                    ZoneType = ZoneType.Storage,
                },
            },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("ZONEGG", result.Value!.Code);
    }
}
