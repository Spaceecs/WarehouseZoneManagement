using Application.ZoneSlots.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.ZoneSlots;

public class BulkCreateSlotsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSlotsWithSequentialCodes()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-030",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONES",
            Name = "Zone S",
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
        var handler = new BulkCreateSlots.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new BulkCreateSlots.Command { ZoneId = zone.Id, Count = 3 },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal("ZONES-S001", result.Value[0].Code);
        Assert.Equal("ZONES-S002", result.Value[1].Code);
        Assert.Equal("ZONES-S003", result.Value[2].Code);
    }

    [Fact]
    public async Task Handle_ShouldContinueNumbering_WhenSlotsAlreadyExist()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-031",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONET",
            Name = "Zone T",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var existingSlot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONET-S001" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(existingSlot);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new BulkCreateSlots.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new BulkCreateSlots.Command { ZoneId = zone.Id, Count = 2 },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("ZONET-S002", result.Value![0].Code);
        Assert.Equal("ZONET-S003", result.Value[1].Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Handle_ShouldReturnFailure_WhenCountIsOutOfRange(int count)
    {
        using var context = TestDbContextFactory.Create();
        var handler = new BulkCreateSlots.Handler(context, MapperFactory.Create());

        var result = await handler.Handle(
            new BulkCreateSlots.Command { ZoneId = Guid.NewGuid(), Count = count },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Count must be between 1 and 100", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenZoneIsInactive()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-032",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEU",
            Name = "Zone U",
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
        var handler = new BulkCreateSlots.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new BulkCreateSlots.Command { ZoneId = zone.Id, Count = 5 },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot generate slots for an inactive zone", result.Error);
    }
}
