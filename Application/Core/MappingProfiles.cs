using Application.Pallets.DTOs;
using Application.Warehouses.DTOs;
using Application.WarehouseZones.DTOs;
using Application.ZoneSlots.DTOs;
using AutoMapper;
using Domain;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreateWarehouseDto, Warehouse>();
        CreateMap<UpdateWarehouseDto, Warehouse>();

        CreateMap<Warehouse, WarehouseListDto>()
            .ForMember(d => d.TotalZones, o => o.MapFrom(s => s.Zones.Count))
            .ForMember(d => d.ActiveZones, o => o.MapFrom(s => s.Zones.Count(z => z.IsActive)));

        CreateMap<Warehouse, WarehouseDetailsDto>()
            .ForMember(
                dest => dest.Zones,
                opt =>
                    opt.MapFrom(src => new WarehouseZonesSummaryDto
                    {
                        Total = src.Zones.Count,

                        Active = src.Zones.Count(z => z.IsActive),

                        TotalSlots = src.Zones.SelectMany(z => z.Slots).Count(),

                        OccupiedSlots = src
                            .Zones.SelectMany(z => z.Slots)
                            .Count(slot => slot.Pallet != null),
                    })
            );

        CreateMap<CreateZoneDto, WarehouseZone>();
        CreateMap<UpdateZoneDto, WarehouseZone>();

        CreateMap<WarehouseZone, ZoneListDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
            .ForMember(dest => dest.TotalSlots, opt => opt.MapFrom(src => src.Slots.Count))
            .ForMember(
                dest => dest.OccupiedSlots,
                opt => opt.MapFrom(src => src.Slots.Count(s => s.Pallet != null))
            );

        CreateMap<WarehouseZone, ZoneDetailsDto>()
            .ForMember(
                dest => dest.Warehouse,
                opt =>
                    opt.MapFrom(src => new ZoneWarehouseShortDto
                    {
                        Id = src.Warehouse.Id,
                        Code = src.Warehouse.Code,
                        Name = src.Warehouse.Name,
                    })
            )
            .ForMember(
                dest => dest.Capacity,
                opt =>
                    opt.MapFrom(src => new ZoneCapacitySummaryDto
                    {
                        Total = src.Slots.Count,
                        Occupied = src.Slots.Count(s => s.Pallet != null),
                        Free = src.Slots.Count(s => s.Pallet == null),
                        UtilizationPercent =
                            src.Slots.Count == 0
                                ? 0
                                : (double)src.Slots.Count(s => s.Pallet != null)
                                    / src.Slots.Count
                                    * 100,
                    })
            );

        CreateMap<ZoneSlot, ZoneSlotDto>()
            .ForMember(dest => dest.IsOccupied, opt => opt.MapFrom(src => src.Pallet != null))
            .ForMember(
                dest => dest.Pallet,
                opt =>
                    opt.MapFrom(src =>
                        src.Pallet != null
                            ? new ZoneSlotPalletShortDto
                            {
                                Id = src.Pallet.Id,
                                Code = src.Pallet.Code,
                            }
                            : null
                    )
            );

        CreateMap<CreatePalletDto, Pallet>();

        CreateMap<Pallet, PalletDto>()
            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(src => src.ZoneSlotId.HasValue ? "Placed" : "Unplaced")
            )
            .ForMember(
                dest => dest.Location,
                opt =>
                    opt.MapFrom(src =>
                        src.ZoneSlot != null
                            ? new PalletLocationDto
                            {
                                SlotId = src.ZoneSlot.Id,
                                SlotCode = src.ZoneSlot.Code,
                                ZoneId = src.ZoneSlot.Zone.Id,
                                ZoneCode = src.ZoneSlot.Zone.Code,
                                ZoneName = src.ZoneSlot.Zone.Name,
                                WarehouseId = src.ZoneSlot.Zone.Warehouse.Id,
                                WarehouseName = src.ZoneSlot.Zone.Warehouse.Name,
                            }
                            : null
                    )
            );
    }
}
