using Application.Pallets.Commands;
using Application.Pallets.DTOs;
using Domain;
using Tests.TestHelpers;

namespace Tests.Pallets;

public class CreatePalletHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreatePallet_WhenCodeIsUnique()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreatePallet.Handler(context, MapperFactory.Create());

        var command = new CreatePallet.Command
        {
            Pallet = new CreatePalletDto { Code = "PLT-100", Description = "Test pallet" },
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Unplaced", result.Value!.Status);
        Assert.Null(result.Value.Location);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeAlreadyExists()
    {
        var dbName = Guid.NewGuid().ToString();

        using (var arrange = TestDbContextFactory.Create(dbName))
        {
            arrange.Pallets.Add(new Pallet { Code = "PLT-DUP" });
            await arrange.SaveChangesAsync();
        }

        using var act = TestDbContextFactory.Create(dbName);
        var handler = new CreatePallet.Handler(act, MapperFactory.Create());

        var result = await handler.Handle(
            new CreatePallet.Command { Pallet = new CreatePalletDto { Code = "PLT-DUP" } },
            CancellationToken.None
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Pallet code must be unique", result.Error);
    }
}
