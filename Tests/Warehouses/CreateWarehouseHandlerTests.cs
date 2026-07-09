using Application.Warehouses.Commands;
using Application.Warehouses.DTOs;
using Tests.TestHelpers;

namespace Tests.Warehouses;

public class CreateWarehouseHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAlwaysCreateWarehouseAsActive_EvenIfClientPassesIsActiveFalse()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateWarehouse.Handler(context, MapperFactory.Create());

        var result = await handler.Handle(
            new CreateWarehouse.Command
            {
                Warehouse = new CreateWarehouseDto
                {
                    Code = "WH-100",
                    Name = "Test Warehouse",
                    IsActive = false,
                },
            },
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsActive);
    }
}
