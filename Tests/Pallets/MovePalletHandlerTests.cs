using Application.Pallets.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.Pallets;

public class MovePalletHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMovePallet_WhenTargetSlotIsFree()
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
            Code = "ZONEM",
            Name = "Zone M",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var sourceSlot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEM-S001" };
        var targetSlot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEM-S002" };
        var pallet = new Pallet { Code = "PLT-020", ZoneSlotId = sourceSlot.Id };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.AddRange(sourceSlot, targetSlot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new MovePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new MovePallet.Command { PalletId = pallet.Id, TargetSlotId = targetSlot.Id },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(targetSlot.Id, result.Value!.Location!.SlotId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPalletIsUnplaced()
    {
        var dbName = Guid.NewGuid().ToString();
        var pallet = new Pallet { Code = "PLT-021" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new MovePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new MovePallet.Command { PalletId = pallet.Id, TargetSlotId = Guid.NewGuid() },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Pallet is unplaced. Use Place instead", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTargetSlotIsOccupied()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-022",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEN",
            Name = "Zone N",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var sourceSlot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEN-S001" };
        var targetSlot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEN-S002" };
        var pallet = new Pallet { Code = "PLT-022", ZoneSlotId = sourceSlot.Id };
        var blockingPallet = new Pallet { Code = "PLT-BLOCK", ZoneSlotId = targetSlot.Id };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.AddRange(sourceSlot, targetSlot);
            arrange.Pallets.AddRange(pallet, blockingPallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new MovePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new MovePallet.Command { PalletId = pallet.Id, TargetSlotId = targetSlot.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Target slot is already occupied", result.Error);
    }
}
