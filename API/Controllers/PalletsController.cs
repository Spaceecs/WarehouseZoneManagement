using Application.Core;
using Application.Pallets.Commands;
using Application.Pallets.DTOs;
using Application.Pallets.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PalletsController : BaseApiController
    {
        public class SlotAssignmentDto
        {
            public Guid SlotId { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<PalletDto>>> GetPallets(
            [FromQuery] GetPalletsList.Query query,
            CancellationToken cancellationToken = default
        )
        {
            return HandleResult(await Mediator.Send(query, cancellationToken));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PalletDto>> GetPallet(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new GetPalletDetails.Query { Id = id }, cancellationToken)
            );
        }

        [HttpPost]
        public async Task<ActionResult<PalletDto>> CreatePallet(
            CreatePalletDto dto,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new CreatePallet.Command { Pallet = dto }, cancellationToken)
            );
        }

        [HttpPost("{id:guid}/place")]
        public async Task<ActionResult<PalletDto>> PlacePallet(
            Guid id,
            SlotAssignmentDto dto,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(
                    new PlacePallet.Command { PalletId = id, SlotId = dto.SlotId },
                    cancellationToken
                )
            );
        }

        [HttpPost("{id:guid}/move")]
        public async Task<ActionResult<PalletDto>> MovePallet(
            Guid id,
            SlotAssignmentDto dto,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(
                    new MovePallet.Command { PalletId = id, TargetSlotId = dto.SlotId },
                    cancellationToken
                )
            );
        }

        [HttpPost("{id:guid}/remove")]
        public async Task<ActionResult<PalletDto>> RemovePallet(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return HandleResult(
                await Mediator.Send(new RemovePallet.Command { PalletId = id }, cancellationToken)
            );
        }
    }
}
