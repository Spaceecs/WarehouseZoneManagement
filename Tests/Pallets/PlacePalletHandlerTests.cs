using Application.Pallets.Commands;
using Domain;
using Tests.TestHelpers;

namespace Tests.Pallets;

public class PlacePalletHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPlacePallet_WhenSlotIsFreeAndZoneIsActive()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var mapper = MapperFactory.Create();

        var warehouse = new Warehouse
        {
            Code = "WH-001",
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
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEA-S001" };
        var pallet = new Pallet { Code = "PLT-001" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new PlacePallet.Handler(act, mapper);
        var command = new PlacePallet.Command { PalletId = pallet.Id, SlotId = slot.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Placed", result.Value!.Status);
        Assert.NotNull(result.Value.Location);
        Assert.Equal(slot.Id, result.Value.Location!.SlotId);
        Assert.Equal(zone.Code, result.Value.Location.ZoneCode);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAttemptingToPlaceIntoFullyOccupiedZone()
    {
        var dbName = Guid.NewGuid().ToString();
        var mapper = MapperFactory.Create();

        var warehouse = new Warehouse
        {
            Code = "WH-002",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEB",
            Name = "Zone B",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEB-S001" };
        var existingPallet = new Pallet { Code = "PLT-EXIST", ZoneSlotId = slot.Id };
        var newPallet = new Pallet { Code = "PLT-NEW" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.AddRange(existingPallet, newPallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new PlacePallet.Handler(act, mapper);
        var command = new PlacePallet.Command { PalletId = newPallet.Id, SlotId = slot.Id };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Slot is already occupied", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPalletDoesNotExist()
    {
        using var act = TestDbContextFactory.Create();
        var handler = new PlacePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new PlacePallet.Command { PalletId = Guid.NewGuid(), SlotId = Guid.NewGuid() },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Pallet not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenZoneIsInactive()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-003",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEC",
            Name = "Zone C",
            ZoneType = ZoneType.Storage,
            IsActive = false,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEC-S001" };
        var pallet = new Pallet { Code = "PLT-003" };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new PlacePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new PlacePallet.Command { PalletId = pallet.Id, SlotId = slot.Id },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot place pallet in an inactive zone", result.Error);
    }
}
