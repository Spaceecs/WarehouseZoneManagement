using Application.Core;
using Application.Pallets.DTOs;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Pallets.Commands;

public class CreatePallet
{
    public class Command : IRequest<Result<PalletDto>>
    {
        public required CreatePalletDto Pallet { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<PalletDto>>
    {
        public async Task<Result<PalletDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var codeExists = await context.Pallets.AnyAsync(
                p => p.Code == request.Pallet.Code,
                cancellationToken
            );

            if (codeExists)
                return Result<PalletDto>.Failure("Pallet code must be unique", 400);

            var pallet = mapper.Map<Pallet>(request.Pallet);

            await context.Pallets.AddAsync(pallet, cancellationToken);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<PalletDto>.Failure("Failed to create pallet", 500);

            var dto = mapper.Map<PalletDto>(pallet);
            return Result<PalletDto>.Success(dto);
        }
    }
}
