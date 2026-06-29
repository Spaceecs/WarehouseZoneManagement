using Application.ZoneSlots.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.ZoneSlots;

public class DeleteSlotHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteSlot_WhenNotOccupied()
    {
        var dbName = Guid.NewGuid().ToString();
        var zone = new WarehouseZone
        {
            WarehouseId = Guid.NewGuid(),
            Code = "ZONEF",
            Name = "Zone F",
            ZoneType = ZoneType.Storage,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEF-S001" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new DeleteSlot.Handler(act);

        var result = await handler.Handle(
            new DeleteSlot.Command { ZoneId = zone.Id, SlotId = slot.Id },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Empty(act.ZoneSlots);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSlotIsOccupied()
    {
        var dbName = Guid.NewGuid().ToString();
        var zone = new WarehouseZone
        {
            WarehouseId = Guid.NewGuid(),
            Code = "ZONEG",
            Name = "Zone G",
            ZoneType = ZoneType.Storage,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEG-S001" };
        var pallet = new Pallet { Code = "PLT-040", ZoneSlotId = slot.Id };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new DeleteSlot.Handler(act);

        var result = await handler.Handle(
            new DeleteSlot.Command { ZoneId = zone.Id, SlotId = slot.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete a slot that is occupied by a pallet", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSlotDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteSlot.Handler(context);

        var result = await handler.Handle(
            new DeleteSlot.Command { ZoneId = Guid.NewGuid(), SlotId = Guid.NewGuid() },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Slot not found", result.Error);
    }
}
