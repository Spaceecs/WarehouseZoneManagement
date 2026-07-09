using API.Controllers;
using Application.Core;
using Microsoft.AspNetCore.Mvc;

namespace Tests.API;

public class TestableApiController : BaseApiController
{
    public ActionResult InvokeHandleResult<T>(Result<T> result) => HandleResult(result);
}

public class HandleResultTests
{
    [Fact]
    public void HandleResult_ShouldReturnNotFound_WhenResultCodeIs404()
    {
        var controller = new TestableApiController();
        var result = Result<string>.Failure("Pallet not found", 404);

        var actionResult = controller.InvokeHandleResult(result);

        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public void HandleResult_ShouldReturnBadRequest_WhenResultCodeIs400()
    {
        var controller = new TestableApiController();
        var result = Result<string>.Failure("Slot is already occupied", 400);

        var actionResult = controller.InvokeHandleResult(result);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public void HandleResult_ShouldReturnOk_WhenResultIsSuccessful()
    {
        var controller = new TestableApiController();
        var result = Result<string>.Success("ok-value");

        var actionResult = controller.InvokeHandleResult(result);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("ok-value", okResult.Value);
    }
}
