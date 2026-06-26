using System;
using Application.Core;
using Application.Warehouses.DTOs;
using AutoMapper;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Warehouses.Commands;

public class CreateWarehouse
{
    public class Command : IRequest<Result<WarehouseDetailsDto>>
    {
        public required CreateWarehouseDto Warehouse { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper)
        : IRequestHandler<Command, Result<WarehouseDetailsDto>>
    {
        public async Task<Result<WarehouseDetailsDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var existing = await context.Warehouses.AnyAsync(x => x.Code == request.Warehouse.Code);
            if (existing)
                return Result<WarehouseDetailsDto>.Failure("Warehouse code must be unique", 401);

            var warehouse = mapper.Map<Warehouse>(request.Warehouse);

            context.Warehouses.Add(warehouse);
            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<WarehouseDetailsDto>.Failure("Failed to create warehouse", 401);

            return Result<WarehouseDetailsDto>.Success(mapper.Map<WarehouseDetailsDto>(warehouse));
        }
    }
}
