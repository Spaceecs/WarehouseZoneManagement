using Application.Core;
using Application.WarehouseZones.Commands;
using Application.WarehouseZones.DTOs;
using Application.WarehouseZones.Queries;
using Application.ZoneSlots.Commands;
using Application.ZoneSlots.DTOs;
using Application.ZoneSlots.Queries;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZonesController : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<PagedList<ZoneListDto>>> GetZones(
            [FromQuery] GetZoneList.Query query,
            CancellationToken cancellationToken = default
        )
        {
            return HandleResult(await Mediator.Send(query, cancellationToken));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ZoneDetailsDto>> GetZoneDetail(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new GetZoneDetails.Query { Id = id }, cancellationToken)
            );
        }

        [HttpPost]
        public async Task<ActionResult<ZoneDetailsDto>> CreateZone(
            CreateZoneDto zoneDto,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new CreateZone.Command { Zone = zoneDto }, cancellationToken)
            );
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ZoneDetailsDto>> UpdateZone(
            Guid id,
            UpdateZoneDto zoneDto,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(
                    new UpdateZone.Command { Id = id, Zone = zoneDto },
                    cancellationToken
                )
            );
        }

        [HttpPost("{id:guid}/activate")]
        public async Task<ActionResult<ZoneDetailsDto>> ActivateZone(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new ActivateZone.Command { Id = id }, cancellationToken)
            );
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<ActionResult<ZoneDetailsDto>> DeactivateZone(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new DeactivateZone.Command { Id = id }, cancellationToken)
            );
        }

        [HttpPost("{id:guid}/slots")]
        public async Task<ActionResult<List<ZoneSlotDto>>> BulkCreateSlots(
            Guid id,
            BulkCreateSlotsDto dto,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(
                    new BulkCreateSlots.Command { ZoneId = id, Count = dto.Count },
                    cancellationToken
                )
            );
        }

        [HttpGet("{id:guid}/slots")]
        public async Task<ActionResult<ZoneSlotDto>> GetZoneSlots(
            Guid id,
            [FromQuery] bool? isOccupied,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(
                    new GetZoneSlotsList.Query { ZoneId = id, IsOccupied = isOccupied },
                    cancellationToken
                )
            );
        }

        [HttpDelete("{id:guid}/slots/{slotId:guid}")]
        public async Task<ActionResult> DeleteSlot(
            Guid id,
            Guid slotId,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(
                    new DeleteSlot.Command { ZoneId = id, SlotId = slotId },
                    cancellationToken
                )
            );
        }
    }
}
