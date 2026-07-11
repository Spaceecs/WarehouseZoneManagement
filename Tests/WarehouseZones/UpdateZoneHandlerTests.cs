using Application.WarehouseZones.Commands;
using Application.WarehouseZones.DTOs;
using Domain;
using Tests.TestHelpers;

namespace Tests.WarehouseZones;

public class UpdateZoneHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCorrectCapacity_AfterUpdate()
    {
        var dbName = Guid.NewGuid().ToString();
        var warehouse = new Warehouse
        {
            Code = "WH-040",
            Name = "Main",
            IsActive = true,
        };
        var zone = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Code = "ZONEC",
            Name = "Zone C",
            ZoneType = ZoneType.Storage,
            IsActive = true,
        };
        var slot = new ZoneSlot { ZoneId = zone.Id, Code = "ZONEC-S001" };
        var pallet = new Pallet { Code = "PLT-040", ZoneSlotId = slot.Id };

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Warehouses.Add(warehouse);
            arrange.WarehouseZones.Add(zone);
            arrange.ZoneSlots.Add(slot);
            arrange.Pallets.Add(pallet);
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new UpdateZone.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new UpdateZone.Command
            {
                Id = zone.Id,
                Zone = new UpdateZoneDto { Name = "Zone C Updated", ZoneType = ZoneType.Cold },
            },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Capacity.Occupied);
    }
}
