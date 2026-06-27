using System.Reflection.PortableExecutable;
using Application.Warehouses.DTOs;
using Application.WarehouseZones.DTOs;
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
    }
}
