using Application.Core;
using Application.Warehouses.Commands;
using Application.Warehouses.DTOs;
using Application.Warehouses.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class WarehousesController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedList<WarehouseListDto>>> GetWarehouses(
        [FromQuery] GetWarehouseList.Query query,
        CancellationToken cancellationToken = default
    )
    {
        return HandleResult(await Mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseDetailsDto>> GetWarehouseById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetWarehouseDetails.Query { Id = id };
        return HandleResult(await Mediator.Send(query, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDetailsDto>> CreateWarehouse(
        CreateWarehouseDto warehouseDto
    )
    {
        return HandleResult(
            await Mediator.Send(new CreateWarehouse.Command { Warehouse = warehouseDto })
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseDetailsDto>> UpdateWarehouse(
        Guid id,
        UpdateWarehouseDto warehouseDto,
        CancellationToken cancellationToken
    )
    {
        return HandleResult(
            await Mediator.Send(
                new UpdateWarehouse.Command { Id = id, Warehouse = warehouseDto },
                cancellationToken
            )
        );
    }
}
