using Application.Pallets.Commands;
using Application.Pallets.DTOs;
using Application.Pallets.Validators;

namespace Tests.Validators;

public class CreatePalletValidatorTests
{
    private readonly CreatePalletValidator _validator = new();

    [Fact]
    public void Should_BeValid_WhenCodeIsCorrect()
    {
        var command = new CreatePallet.Command
        {
            Pallet = new CreatePalletDto { Code = "PLT-0001" },
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_BeInvalid_WhenCodeContainsLowercaseLetters()
    {
        var command = new CreatePallet.Command
        {
            Pallet = new CreatePalletDto { Code = "plt-0001" },
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == "Code must contain only uppercase letters, numbers, and hyphens"
        );
    }

    [Fact]
    public void Should_BeInvalid_WhenDescriptionTooLong()
    {
        var command = new CreatePallet.Command
        {
            Pallet = new CreatePalletDto { Code = "PLT-0001", Description = new string('a', 201) },
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == "Description cannot exceed 200 characters"
        );
    }
}
