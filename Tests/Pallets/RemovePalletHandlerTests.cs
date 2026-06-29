using Application.Pallets.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.Pallets;

public class RemovePalletHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemovePallet_WhenPlaced()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-050",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONER",
            Name = "Zone R",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONER-S001" };
        var pallet = new Pallet { Code = "PLT-050", ZoneSlotId = slot.Id };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new RemovePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new RemovePallet.Command { PalletId = pallet.Id },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("Unplaced", result.Value!.Status);
        Assert.Null(result.Value.Location);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPalletIsAlreadyUnplaced()
    {
        var dbName = Guid.NewGuid().ToString();
        var pallet = new Pallet { Code = "PLT-051" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new RemovePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new RemovePallet.Command { PalletId = pallet.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Pallet is already unplaced", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPalletDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new RemovePallet.Handler(context, MapperFactory.Create());

        var result = await handler.Handle(
            new RemovePallet.Command { PalletId = Guid.NewGuid() },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Pallet not found", result.Error);
    }
}
