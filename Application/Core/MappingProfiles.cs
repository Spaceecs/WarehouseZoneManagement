using Application.Warehouses.DTOs;
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
    }
}
